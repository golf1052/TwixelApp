using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TwixelApp.Constants;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using WinRTXamlToolkit.Controls.Extensions;

namespace TwixelApp
{
    public class ChatWindow
    {
        public Client client;
        MessageHandlerListener messageListener;
        bool keyPressed;
        ObservableCollection<ChatListViewBinding> chatMessages;
        bool lockToBottom;
        ScrollBar verticalBarBar;
        CoreDispatcher Dispatcher;
        string channelName;
        bool sentJoin;
        string loginName;

        Grid chatGrid;
        ListView chatView;
        TextBox chatBox;
        Button chatSendButton;

        Random random;
        bool canChat;
        bool loadedChatView = false;
        public bool connectedToChat = false;

        List<string> users = new List<string>();
        List<string> mods = new List<string>();

        SubscriberEmote channelEmotes;

        public ChatWindow(CoreDispatcher Dispatcher,
            string channelName,
            Grid chatGrid,
            ListView chatView,
            TextBox chatBox,
            Button chatSendButton)
        {
            this.channelName = channelName;
            this.Dispatcher = Dispatcher;
            this.chatGrid = chatGrid;
            this.chatView = chatView;
            this.chatBox = chatBox;
            this.chatSendButton = chatSendButton;
            keyPressed = false;
            lockToBottom = true;
            sentJoin = false;
            canChat = false;
            chatMessages = new ObservableCollection<ChatListViewBinding>();
            client = new Client(channelName);
            //client = new Client("golf1052");
            messageListener = new MessageHandlerListener();
            random = new Random();
            chatView.Loaded += chatView_Loaded;
            client.Message += client_Message;
            chatSendButton.Click += chatSendButton_Click;
            chatBox.KeyDown += chatBox_KeyDown;
            foreach (SubscriberEmote subEmotes in AppConstants.subscriberEmotes)
            {
                if (channelName == subEmotes.channelName)
                {
                    channelEmotes = subEmotes;
                    break;
                }
            }
        }

        public async Task LoadChatWindow()
        {
            await client.Connect();
            if (chatView != null)
            {
                if (!loadedChatView)
                {
                    loadedChatView = true;
                    LoadChatView();
                }
            }

            if (AppConstants.ActiveUser != null)
            {
                if (AppConstants.ActiveUser.authorized)
                {
                    if (AppConstants.ActiveUser.authorizedScopes.Contains(TwixelAPI.Constants.TwitchConstants.Scope.ChatLogin))
                    {
                        canChat = true;
                        await client.Login(AppConstants.ActiveUser.name, AppConstants.ActiveUser.accessToken);
                        loginName = AppConstants.ActiveUser.name;
                        return;
                    }
                }
            }

            await ConnectAnon();
        }

        async void chatBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!keyPressed)
                {
                    keyPressed = true;
                    await SendChatMessage();
                    keyPressed = false;
                }
            }
        }

        async void chatSendButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await SendChatMessage();
        }

        void chatView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (!loadedChatView)
            {
                loadedChatView = true;
                LoadChatView();
            }
        }

        void LoadChatView()
        {
            chatView.ItemsSource = chatMessages;
            AddChatMessage("", "Connecting to chat...", false, false);
            chatMessages.CollectionChanged += (s, args) => ScrollToBottom();
            var scrollViewer = chatView.GetFirstDescendantOfType<ScrollViewer>();
            var scrollBars = scrollViewer.GetDescendantsOfType<ScrollBar>().ToList();
            var verticalBar = scrollBars.FirstOrDefault(x => x.Orientation == Orientation.Vertical);
            verticalBarBar = verticalBar;

            if (verticalBarBar != null)
            {
                verticalBarBar.Scroll += verticalBarBar_Scroll;
            }
        }

        void ScrollToBottom()
        {
            var scrollViewer = chatView.GetFirstDescendantOfType<ScrollViewer>();
            scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
            //if (!lockToBottom)
            //{
            //    return;
            //}

            //int selectedIndex = chatView.Items.Count - 1;
            //if (selectedIndex < 0)
            //{
            //    return;
            //}
            //chatView.ScrollIntoView(chatView.Items[selectedIndex]);
        }

        private void verticalBarBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollEventType != ScrollEventType.EndScroll)
            {
                return;
            }

            ScrollBar bar = sender as ScrollBar;
            if (bar == null)
            {
                return;
            }

            if (e.NewValue >= bar.Maximum)
            {
                lockToBottom = true;
            }
            else
            {
                lockToBottom = false;
            }
        }  

        public async Task ConnectAnon()
        {
            canChat = false;
            chatBox.Text = "Not logged in";
            chatBox.IsEnabled = false;
            chatSendButton.IsEnabled = false;
            int randomNumber = random.Next(100000000, 1000000000);
            await client.SetNick("justinfan" + randomNumber);
            loginName = "justinfan" + randomNumber;
        }

        async Task SendChatMessage()
        {
            if (chatBox.Text != "")
            {
                await client.SendIRCMessage(chatBox.Text);
                AddChatMessage(loginName, chatBox.Text, true, true);
                chatBox.Text = "";
                chatView.UpdateLayout();
            }
        }

        void AddChatMessage(string name, string message, bool checkForEmotes, bool boldName)
        {
            if (!checkForEmotes)
            {
                TextBlock textBlockMessage = new TextBlock();
                textBlockMessage.Text = message;
                textBlockMessage.TextWrapping = TextWrapping.WrapWholeWords;
                ObservableCollection<UIElement> elements = new ObservableCollection<UIElement>();
                elements.Add(textBlockMessage);
                chatMessages.Add(new ChatListViewBinding(name, elements, boldName));
            }
            else
            {
                string[] splitMessage = message.Split(' ');
                if (splitMessage.Length == 1)
                {
                    object returned = CheckForEmote(splitMessage[0]);
                    if (returned.GetType() == typeof(string))
                    {
                        TextBlock textBlockMessage = new TextBlock();
                        textBlockMessage.Text = splitMessage[0];
                        textBlockMessage.TextWrapping = TextWrapping.WrapWholeWords;
                        ObservableCollection<UIElement> elements = new ObservableCollection<UIElement>();
                        elements.Add(textBlockMessage);
                        chatMessages.Add(new ChatListViewBinding(name, elements, boldName));
                    }
                    else
                    {
                        ObservableCollection<UIElement> elements = new ObservableCollection<UIElement>();
                        elements.Add((Image)returned);
                        chatMessages.Add(new ChatListViewBinding(name, elements, boldName));
                    }
                }
                else
                {
                    string accumulatedString = "";
                    ObservableCollection<UIElement> elements = new ObservableCollection<UIElement>();
                    foreach (string s in splitMessage)
                    {
                        object returned = CheckForEmote(s);
                        if (returned.GetType() == typeof(string))
                        {
                            accumulatedString += (string)returned + " ";
                        }
                        else
                        {
                            if (accumulatedString != "")
                            {
                                TextBlock t = new TextBlock();
                                t.Text = accumulatedString;
                                t.TextWrapping = TextWrapping.WrapWholeWords;
                                elements.Add(t);
                                accumulatedString = "";
                            }

                            elements.Add((Image)returned);
                        }
                    }

                    if (accumulatedString != "")
                    {
                        TextBlock t = new TextBlock();
                        t.Text = accumulatedString;
                        t.TextWrapping = TextWrapping.WrapWholeWords;
                        elements.Add(t);
                        accumulatedString = "";
                    }

                    chatMessages.Add(new ChatListViewBinding(name, elements, boldName));
                }
            }
            //TextBlock textBlock = new TextBlock();
            //textBlock.Text = "test";
            //Image image = new Image();
            //BitmapImage bitmapImage = new BitmapImage(new Uri("http://static-cdn.jtvnw.net/jtv_user_pictures/chansub-global-emoticon-ddc6e3a8732cb50f-25x28.png"));
            //image.Source = bitmapImage;
            //ObservableCollection<UIElement> elements = new ObservableCollection<UIElement>();
            //elements.Add(textBlock);
            //elements.Add(image);
            //chatMessages.Add(new ChatListViewBinding("test", elements, false));
        }

        object CheckForEmote(string s)
        {
            foreach (Emote emote in AppConstants.emotes)
            {
                if (emote.name == s)
                {
                    return CreateEmoteImage(emote);
                }
            }

            if (channelEmotes != null)
            {
                foreach (Emote emote in channelEmotes.emotes)
                {
                    if (emote.name == s)
                    {
                        return CreateEmoteImage(emote);
                    }
                }
            }

            foreach (SubscriberEmote subEmotes in AppConstants.subscriberEmotes)
            {
                foreach (Emote emote in subEmotes.emotes)
                {
                    if (emote.name == s)
                    {
                        return CreateEmoteImage(emote);
                    }
                }
            }

            return s;
        }

        Image CreateEmoteImage(Emote emote)
        {
            Image image = new Image();
            BitmapImage bitmapImage = new BitmapImage(emote.url.url);
            image.Source = bitmapImage;
            string[] spliturl = emote.url.urlString.Split('-');
            string[] splitSize = spliturl[spliturl.Length - 1].Split('.');
            string[] sizes = splitSize[0].Split('x');
            image.Width = int.Parse(sizes[0]);
            image.Height = int.Parse(sizes[1]);
            return image;
        }

        async void client_Message(object source, MessageHandlerEventArgs e)
        {
            if (!sentJoin)
            {
                if (e.Message.Contains("376"))
                {
                    await client.SendJoin();
                    sentJoin = true;
                }
            }

            string ircMessagePrefix = ":" + loginName + ".tmi.twitch.tv ";
            string modsMessagePrefix = ":jtv MODE #" + channelName + " +o ";
            if (e.Message.StartsWith(ircMessagePrefix))
            {
                string ircCommand = e.Message.Substring(ircMessagePrefix.Length, 3);
                if (ircCommand == "353")
                {
                    string tmpUsersString = e.Message.Substring((ircMessagePrefix + "353 " + loginName + " = #" + channelName + " :").Length);
                    string[] tmpUsers = tmpUsersString.Split(' ');
                    foreach (string user in tmpUsers)
                    {
                        users.Add(user);
                    }
                }

                if (ircCommand == "366")
                {
                    await client.SendWho();
                }
            }

            if (e.Message.StartsWith(modsMessagePrefix))
            {
                mods.Add(e.Message.Substring(modsMessagePrefix.Length));
            }

            string name = "";
            string chatMessage = "";
            if (!e.Message.Contains("JOIN #" + channelName) && !e.Message.Contains("PART #" + channelName))
            {
                //Debug.WriteLine(e.Message);
            }

            if (e.Message.Contains("PRIVMSG"))
            {
                int indexOf = e.Message.IndexOf("PRIVMSG");
                string userNamePart = "";
                string rest = "";
                if (indexOf >= 0)
                {
                    try
                    {
                        userNamePart = e.Message.Substring(0, indexOf - 1);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    rest = e.Message.Substring(indexOf + "PRIVMSG".Length + 1);
                }
                string msg = "";
                chatMessage = rest;

                if (rest.StartsWith(loginName))
                {
                    // this is a message to us
                    chatMessage = rest;
                }
                else
                {
                    // this is a message from someone else
                    int indexOfChannel = rest.IndexOf(channelName);
                    if (indexOfChannel >= 0)
                    {
                        msg = rest.Substring(indexOfChannel + channelName.Length + 2);
                    }
                }

                string[] splitUsername = userNamePart.Split('!');
                if (splitUsername.Length > 1)
                {
                    string[] realUsername = splitUsername[1].Split('@');
                    name = realUsername[0];
                }
                chatMessage = msg;
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (name == "jtv")
                {
                    if (!connectedToChat)
                    {
                        connectedToChat = true;
                        AddChatMessage("", "Connected to chat", false, false);

                        if (AppConstants.ActiveUser != null)
                        {
                            if (AppConstants.ActiveUser.authorized)
                            {
                                chatBox.IsEnabled = true;
                                chatSendButton.IsEnabled = true;
                            }
                        }
                    }
                    //Debug.WriteLine("JTV SAID: " + chatMessage);
                }

                if (name != "" && chatMessage != "")
                {
                    if (name != "jtv")
                    {
                        bool boldName = name == channelName;
                        AddChatMessage(name, chatMessage, true, boldName);
                        chatView.UpdateLayout();
                    }
                }
            });
        }
    }
}
