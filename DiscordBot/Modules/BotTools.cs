﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

public class BotTools : InteractiveBase
{
    /// <summary>
    /// Int 0 is to modify the message, int 1 is to send a new message
    /// </summary>
    public static async Task NotificationControlAsync(ulong messageid, ulong channelid, string msg, int status,
        int num = 0)
    {
        try
        {
            if (num == 0)
            {
                IMessageChannel channel = (IMessageChannel) KKK.Client.GetChannel(channelid);
                IUserMessage themessage = (IUserMessage) await channel.GetMessageAsync(messageid);

                await themessage.ModifyAsync(msgProperty => { msgProperty.Embed = Embed(msg, status); });
            }
            else if (num == 1)
            {
                IMessageChannel channel = (IMessageChannel) KKK.Client.GetChannel(channelid);

                await channel.SendMessageAsync(null, false, Embed(msg, status));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public static Embed Embed(string msg, int status = 0)
    {
        var ebd = new EmbedBuilder();
        if (status == 0)
        {
            Color Colorr = new Color(21, 22, 34);
            ebd.Color = Colorr;
        }
        else if (status == 20 || status == 21)
        {
            ebd.Color = Color.Green;
        }
        else if (status == 40 || status == 44)
        {
            ebd.Color = Color.Red;
        }

        ebd.WithDescription($"{msg}");

        return ebd.Build();
    }

    private static Server server = new Server();

    /// <summary>
    /// By giving server id, it gets ip and token, and using timout is optional, its on 10 sec default
    /// </summary>
    public static async Task<int> Sendmsg(ulong serverid, string msg, long timeout = 10000)
    {
        string token = server.GetTokenOfServer(serverid);
        string ip = server.GetIPForToken(token, 2);

        if (CheckConnection(ip)) //check if its connected
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                if (sw.ElapsedMilliseconds > timeout)
                {
                    Console.WriteLine($"Connection Timeout for {ip}");
                    return 0;
                }

                try
                {
                    var socket = DiscordBot.allSockets.Find(client => client.ConnectionInfo.ClientIpAddress == ip);
                    await socket.Send(msg);
#if DEBUG
                    Console.WriteLine($"{msg} send to {socket.ConnectionInfo.ClientIpAddress}");
#endif
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return 0;
                }
            }
        }

        return 0;
    }

    [Command("rec", RunMode = RunMode.Async)]
    [Alias("rec")]
    [Summary("Recreates Fork related role and channel")]
    public async Task Rec()
    {
        await Task.Run(async () =>
        {
            try
            {
                server.RemoveRole(Context.Guild.Id);
                string warning = null;
                try
                {
                    if (Context.Guild.Roles.Any(x => x.Name.ToLower() == "Fork-Mods".ToLower()))
                    {
                        foreach (var Role in Context.Guild.Roles.Where(x => x.Name.ToLower() == "Fork-Mods".ToLower()))
                        {
                            await Role.DeleteAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    warning +=
                        $"`Fork-Mods` role detected, please move my role to top roles then run `$rec` to clean it." +
                        Environment.NewLine;
                }

                try
                {
                    foreach (var channel in Context.Guild.Channels.Where(x =>
                        string.Equals(x.Name, "Fork-Bot", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        await channel.DeleteAsync();
                    }
                }
                catch (Exception ex)
                {
                    warning +=
                        $"`Fork-Bot` channel detected, please move my role to top roles then run `$rec` to clean it." +
                        Environment.NewLine;
                }

                if (warning == null)
                {
                    ulong origin = (ulong) GuildPermission.Speak + (ulong) GuildPermission.SendTTSMessages +
                                   (ulong) GuildPermission.SendMessages + (ulong) GuildPermission.ViewChannel +
                                   (ulong) GuildPermission.EmbedLinks + (ulong) GuildPermission.Connect +
                                   (ulong) GuildPermission.AttachFiles + (ulong) GuildPermission.AddReactions;
                    GuildPermissions perms = new GuildPermissions(origin);
                    //Color Colorr = new Color(21, 22, 34);
                    var roleee = await Context.Guild.CreateRoleAsync("Fork-Mods", perms, null, false, false, null);
                    var vChan = await Context.Guild.CreateTextChannelAsync("Fork-Bot");
                    await vChan.AddPermissionOverwriteAsync(roleee, CommandHandler.AdminPermissions());
                    await vChan.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, CommandHandler.NoPermissions());

                    var ebd = new EmbedBuilder();
                    ebd.Color = Color.Green;
                    ebd.WithCurrentTimestamp();
                    ebd.WithAuthor($"Fork Server Management", Context.Guild.CurrentUser.GetAvatarUrl());
                    ebd.WithDescription(
                        "Hello there," +
                        Environment.NewLine +
                        "I'm Fork Bot if you don't know me, I can help you control your Fork Minecraft servers and display their status in Discord." +
                        Environment.NewLine +
                        "I made a private channel for you, please use `$auth [token]` to link this Discord server with your Fork app." +
                        Environment.NewLine +
                        "You can check for your token in Fork app settings.");
                    //var ownerr = KKK.Client.GetGuild(guild.Id).OwnerId;
                    await vChan.SendMessageAsync($"<@{Context.Guild.OwnerId}>", false, ebd.Build());
                    var msgg = await vChan.SendMessageAsync(null, false,
                        Embed(
                            "Don't remove this message, this message will be updated continuously and display the status of you Fork servers.",
                            20));
                    server.InsertRole(Context.Guild.Id, roleee.Id, vChan.Id, msgg.Id);
                }
                else
                {
                    var ebd = new EmbedBuilder();
                    ebd.Color = Color.Red;
                    ebd.WithCurrentTimestamp();
                    ebd.WithAuthor($"Error", Context.Guild.CurrentUser.GetAvatarUrl());
                    ebd.WithDescription(warning);
                    //var ownerr = KKK.Client.GetGuild(guild.Id).OwnerId;
                    await Context.Guild.DefaultChannel.SendMessageAsync($"<@{Context.Guild.OwnerId}>", false,
                        ebd.Build());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        });
    }

    [Command("stop", RunMode = RunMode.Async)]
    [Alias("stop")]
    [Summary("Stops a specific Minecraft server")]
    public async Task Stop(string servername)
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            int result = await Sendmsg(Context.Guild.Id,
                $"stop|{servername}|{Context.User.Username}#{Context.User.Discriminator}|{Context.Channel.Id}|{msg.Id}");
            switch (result)
            {
                case 1:
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed = Embed("Command Executed.", 20);
                    });
                    break;
                case 0:
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed =
                            Embed(
                                "Oops. Looks like your fork app isn't online or connection timed out, please restart it.",
                                40);
                    });
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Command("start", RunMode = RunMode.Async)]
    [Alias("start")]
    [Summary("Starts a specific Minecraft server")]
    public async Task Start(string servername)
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            int result = await Sendmsg(Context.Guild.Id,
                $"start|{servername}|{Context.User.Username}#{Context.User.Discriminator}|{Context.Channel.Id}|{msg.Id}");
            switch (result)
            {
                case 1:
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed = Embed("Command Executed.", 20);
                    });
                    break;
                case 0:
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed =
                            Embed(
                                "Oops. Looks like your fork app isnt online or connection timed out, please restart it.",
                                40);
                    });
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Command("notify", RunMode = RunMode.Async)]
    [Alias("notification")]
    [Summary("Subscribe to Player join/leave events")]
    public async Task Notify(SocketGuildChannel channel)
    {
        try
        {
            string token = server.GetTokenOfServer(Context.Guild.Id);
            string ip = server.GetIPForToken(token, 2);
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            if (server.CheckIfNotifyExist(Context.Guild.Id))
            {
                //await msgg.PinAsync();
                server.UpdateNotify(Context.Guild.Id, channel.Id);
                string warn = null;
                if (CheckConnection(ip))
                {
                    await Sendmsg(Context.Guild.Id, $"subscribe|playerEvent");
                }
                else
                {
                    warn = Environment.NewLine +
                           "Couldn't connect to your Fork app but don't worry, I'll send you updates as soon as it's connected again.";
                }

                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed($"Notification channel updated.{warn}", 20);
                });
            }
            else if ((bool) server.CheckIfNotifyExist(Context.Guild.Id) == false)
            {
                //await msgg.PinAsync();
                server.InsertNotify(Context.Guild.Id, channel.Id);
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Notification channel submitted.", 20);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: " + ex.Message);
        }
    }

    [Command("dnotify", RunMode = RunMode.Async)]
    [Alias("dnotification")]
    [Summary("Unsubscribes from Player join/leave events")]
    public async Task DNotify()
    {
        try
        {
            string token = server.GetTokenOfServer(Context.Guild.Id);
            string ip = server.GetIPForToken(token, 2);
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            if (server.CheckIfNotifyExist(Context.Guild.Id))
            {
                server.RemoveNotify(Context.Guild.Id);
                if (CheckConnection(ip))
                {
                    await Sendmsg(Context.Guild.Id, $"unsub|playerEvent");
                }

                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Unsubscribed from player notifications successfully.", 20);
                });
            }
            else if (!server.CheckIfNotifyExist(Context.Guild.Id))
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("You are not subscribed to player notifications.", 40);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: " + ex.Message);
        }
    }

    //[Command("sub", RunMode = RunMode.Async)]
    //[Alias("subscribe")]
    //[Summary("Subscribe to an event. usage: (prefix)sub")]
    public async Task Sub(bool check)
    {
        try
        {
            string token = server.GetTokenOfServer(Context.Guild.Id);
            string ip = server.GetIPForToken(token, 2);
            int truefalse = check ? 1 : 0;
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            if (server.CheckSevent(Context.Guild.Id))
            {
                if (check)
                {
                    if (!server.CheckSevent(Context.Guild.Id, 1))
                    {
                        IMessageChannel chan =
                            (IMessageChannel) Context.Guild.GetChannel((ulong) server.GetSeventCH(Context.Guild.Id, 0));
                        var msgg = await chan.SendMessageAsync(null, false,
                            Embed(
                                "Don't remove this message, this message will be updated continuously and display the status of you Fork servers.",
                                20));
                        server.UpdateSEvent(Context.Guild.Id, msgg.Id, truefalse);
                        string warn = null;
                        if (CheckConnection(ip))
                        {
                            await Sendmsg(Context.Guild.Id, $"subscribe|playerEvent");
                        }
                        else
                        {
                            warn = Environment.NewLine +
                                   "Couldn't connect to your Fork app but dont worry, I'll send updates as soon as it is connected again.";
                        }

                        await msg.ModifyAsync(msgProperty =>
                        {
                            msgProperty.Content = $"{Context.Message.Author.Mention}";
                            msgProperty.Embed = Embed($"Enabled successfully.{warn}", 20);
                        });
                    }
                    else
                    {
                        await msg.ModifyAsync(msgProperty =>
                        {
                            msgProperty.Content = $"{Context.Message.Author.Mention}";
                            msgProperty.Embed = Embed($"Already enabled.", 40);
                        });
                    }
                }
                else if (!check)
                {
                    if (server.CheckSevent(Context.Guild.Id, 1))
                    {
                        server.UpdateSEvent(Context.Guild.Id, 0, truefalse);
                        if (CheckConnection(ip) == true)
                        {
                            await Sendmsg(Context.Guild.Id, $"unsub|serverListEvent");
                        }

                        await msg.ModifyAsync(msgProperty =>
                        {
                            msgProperty.Content = $"{Context.Message.Author.Mention}";
                            msgProperty.Embed = Embed($"Disabled successfully.", 20);
                        });
                    }
                    else
                    {
                        await msg.ModifyAsync(msgProperty =>
                        {
                            msgProperty.Content = $"{Context.Message.Author.Mention}";
                            msgProperty.Embed = Embed($"Already disabled.", 40);
                        });
                    }
                }
            }
            else
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed($"Please use $rec and retry", 40);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: " + ex.Message);
        }
    }

    //[Command("ping", RunMode = RunMode.Async)]
    //[Alias("latency")]
    //[Summary("Shows the websocket connection's latency and time it takes to send a message. usage: (prefix)ping")]
    public async Task PingAsync()
    {
        try
        {
            var watch = Stopwatch.StartNew();
            var msg = await ReplyAsync("Pong");
            await msg.ModifyAsync(msgProperty => msgProperty.Content = $"🏓 {watch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: " + ex.Message);
        }
    }

    [Command("auth", RunMode = RunMode.Async)]
    [Alias("authorize")]
    [Summary("Links your Discord server to your Fork app")]
    public async Task Auth(string token)
    {
        try
        {
            await Context.Message.DeleteAsync();
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            if (!server.CheckOnhold(token))
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed =
                        Embed(
                            "Couldn't connect to your Fork app. Either it is not running, Fork Bot is not enabled or your Fork app is already linked to another Discord server.",
                            40);
                });
            }
            else if (!server.CheckAuth(token, Context.Guild.Id) && server.CheckOnhold(token))
            {
                //sorting the token goes here
                //After connection if server replies, then its ok
                string ip = server.GetIPForToken(token, 1);
                if (CheckConnection(ip)) //check if its connected
                {
                    server.InsertAuth(Context.Guild.Id, token, ip);
                    server.RemoveOnhold(token);
                    await Sendmsg(Context.Guild.Id, $"status|Linked|{Context.Guild.Name}");
                    if (!server.CheckRoleAndChannel(Context.Guild.Id))
                    {
                        await Rec();
                    }

                    await Sendmsg(Context.Guild.Id, $"subscribe|serverListEvent");

                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed = Embed("Great, your discord server is now linked to your Fork app.",
                            20);
                    });
                }
                else
                {
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed = Embed("Couldn't connect to your Fork app, make sure it's running.", 40);
                    });
                }
            }
            else
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed =
                        Embed("Sorry, but this Discord server or the token is already authorized or invalid.", 40);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: " + ex.Message);
        }
    }

    [Command("help", RunMode = RunMode.Async)]
    [Summary("Lists all available commands")]
    public async Task Help()
    {
        try
        {
            EmbedBuilder eb = new EmbedBuilder {Color = Color.Blue};
            bool onemod = false;
            List<CommandInfo> commands = new();
            foreach (ModuleInfo modulename in KKK.CommandService.Modules)
            {
                commands.AddRange(modulename.Commands);
            }
            commands.Sort((a, b) => a.CustomPriority().CompareTo(b.CustomPriority()));
            StringBuilder names = new();
            StringBuilder descriptions = new();
            foreach (CommandInfo command in commands)
            {
                string cmdName = command.Name;
                if (command.Parameters.Count > 0)
                {
                    cmdName += " [";
                    for (int i = 0; i < command.Parameters.Count; i++)
                    {
                        cmdName += command.Parameters[i];
                        if (i < command.Parameters.Count - 1)
                        {
                            cmdName += " ";
                        }
                    }
                    cmdName += "]";
                }
                names.Append($"${cmdName}{Environment.NewLine}");
                descriptions.Append(command.Summary + Environment.NewLine);
            }

            eb.AddField("Command", names.ToString(), true);
            eb.AddField("Description", descriptions.ToString(), true);

            eb.WithCurrentTimestamp();
            eb.WithAuthor("Command List", Context.Client.CurrentUser.GetAvatarUrl());
            eb.WithFooter("Requested by: " + Context.User.Username);
            await ReplyAsync(Context.Message.Author.Mention, false, eb.Build());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: " + ex.Message);
        }
    }

    [Command("leave", RunMode = RunMode.Async)]
    [Alias("leave")]
    [Summary("Bot will leave this Discord server")]
    public async Task Leave()
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed(
                    $"Please type `{Context.Guild.Name}` to confirm.{Environment.NewLine}Be aware this process cant be recovered.{Environment.NewLine}Type anything else to cancel."));
            var msgg = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));
            await ReplyAsync(Context.Message.Author.Mention, false,
                Embed($"Sad to see you go.., I'll leave shortly, good bye!", 20));
            if (msgg.Content == Context.Guild.Name)
            {
                if (server.CheckRoleAndChannel(Context.Guild.Id))
                {
                    try
                    {
                        if (Context.Guild.GetChannel((ulong) server.GetRoleandChannel(Context.Guild.Id, 1)) != null)
                        {
                            var channel =
                                Context.Guild.GetChannel((ulong) server.GetRoleandChannel(Context.Guild.Id, 1));
                            await channel.DeleteAsync();
                        }

                        if (Context.Guild.GetRole((ulong) server.GetRoleandChannel(Context.Guild.Id, 0)) != null)
                        {
                            var Role = Context.Guild.GetRole((ulong) server.GetRoleandChannel(Context.Guild.Id, 0));
                            await Role.DeleteAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception occured: " + ex.Message);
                    }
                }

                await Sendmsg(Context.Guild.Id, $"rec");
                await Sendmsg(Context.Guild.Id, $"status|OnHold");
                await Sendmsg(Context.Guild.Id, $"unsub|serverListEvent");
                await Sendmsg(Context.Guild.Id, $"unsub|playerEvent");
                server.LeaveServer(Context.Guild.Id);
                try
                {
                    await Context.Guild.LeaveAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occured: " + ex.Message);
                }
            }
            else
            {
                await ReplyAsync(Context.Message.Author.Mention, false, Embed($"Canceled.", 20));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static bool CheckConnection(string ip)
    {
        if (DiscordBot.allSockets.Any(client => client.ConnectionInfo.ClientIpAddress == ip))
        {
            var socket = DiscordBot.allSockets.Find(client => client.ConnectionInfo.ClientIpAddress == ip);
            return socket != null && socket.IsAvailable;
        }

        return false;
    }

    [Command("unauth", RunMode = RunMode.Async)]
    [Alias("unauthorize")]
    [Summary("Unlinks your Fork app from this Discord server")]
    public async Task UnAuth()
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            string token = server.GetTokenOfServer(Context.Guild.Id);
            if (server.CheckAuth(token, Context.Guild.Id))
            {
                await Sendmsg(Context.Guild.Id, $"rec");
                await Sendmsg(Context.Guild.Id, $"status|OnHold");
                await Sendmsg(Context.Guild.Id, $"unsub|serverListEvent");
                await Sendmsg(Context.Guild.Id, $"unsub|playerEvent");
                server.LeaveServer(Context.Guild.Id);
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Your discord server got unlinked from your Fork app successfully.", 20);
                });
            }
            else
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Your discord server isn't linked to a Fork app.", 40);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: " + ex.Message);
        }
    }
}