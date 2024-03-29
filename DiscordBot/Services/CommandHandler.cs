﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models;
using NLog;

namespace DiscordBot
{

    static class KKK
    {
        public static string prefix = "$";
        public static CommandService CommandService;
        public static DiscordSocketClient Client;
        public static bool IsClientReady = false;
    }

    public class CommandHandler
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IServiceProvider Services;

        public CommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider services)
        {
            KKK.Client = client;
            KKK.CommandService = commandService;
            Services = services;
        }

        public async Task InitializeAsync()
        {
            await KKK.CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
            KKK.Client.Ready += OnClientReady;
            KKK.Client.UserJoined += welcome;
            KKK.Client.MessageReceived += HandleCommandAsync;
            KKK.Client.JoinedGuild += OnGuildJoin;
            KKK.Client.LeftGuild += OnGuildLeave;
            KKK.Client.Disconnected += OnClientDisconnect;
        }

        public static OverwritePermissions AdminPermissions()
        {
            return new(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow,
                PermValue.Allow, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Allow,
                PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny,
                PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
        }

        public static OverwritePermissions NoPermissions()
        {
            return new(PermValue.Deny, PermValue.Deny, PermValue.Deny,
                PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny,
                PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny,
                PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny,
                PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny,
                PermValue.Deny);
        }

        private async Task OnGuildJoin(SocketGuild guild)
        {
            await Task.Run(async () =>
            {
                try
                {
                    DatabaseContext context = new DatabaseContext();


                    if (!(context.Auth.Any(o => o.Serverid == guild.Id) && (!(context.OnJoin.Any(o => o.Serverid == guild.Id)))))
                    {
                        string warning = null;
                        try
                        {
                            if (guild.Roles.Any(x => x.Name == "Fork-Mods"))
                            {
                                foreach (var Role in guild.Roles.Where(x => x.Name == "Fork-Mods"))
                                {
                                    await Role.DeleteAsync();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            warning +=
                                $"`Fork-Mods` role detected, please move my role above the `Fork-Mods` role and authenticate using `$auth [token]` then run `$rec` to clean it." +
                                Environment.NewLine;
                        }

                        try
                        {
                            if (guild.TextChannels.Any(x => x.Name == "Fork-Bot"))
                            {
                                foreach (var Chan in guild.Channels.Where(x => x.Name == "Fork-Bot"))
                                {
                                    await Chan.DeleteAsync();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            warning +=
                                $"`Fork-Bot` channel detected, please move my role above the `Fork-Mods` role and authenticate using `$auth [token]` then run `$rec` to clean it." +
                                Environment.NewLine;
                        }

                        if (warning == null)
                        {
                            ulong origin = (ulong)GuildPermission.Speak + (ulong)GuildPermission.SendTTSMessages +
                                           (ulong)GuildPermission.SendMessages + (ulong)GuildPermission.ViewChannel +
                                           (ulong)GuildPermission.EmbedLinks + (ulong)GuildPermission.Connect +
                                           (ulong)GuildPermission.AttachFiles + (ulong)GuildPermission.AddReactions;
                            GuildPermissions perms = new GuildPermissions(origin);
                        //Color Colorr = new Color(21, 22, 34);
                        var roleee = await guild.CreateRoleAsync("Fork-Mods", perms, null, false, false, null);
                            var vChan = await guild.CreateTextChannelAsync("Fork-Bot");
                            await vChan.AddPermissionOverwriteAsync(roleee, AdminPermissions());
                            await vChan.AddPermissionOverwriteAsync(guild.EveryoneRole, NoPermissions());

                            var ebd = new EmbedBuilder();
                            ebd.Color = Color.Green;
                            ebd.WithCurrentTimestamp();
                            ebd.WithAuthor($"Fork Server Management", guild.CurrentUser.GetAvatarUrl());
                            ebd.WithDescription(
                                "Hello there," +
                                Environment.NewLine +
                                "I'm Fork Bot if you don't know me, I can help you control your Fork Minecraft servers and display their status in Discord." +
                                Environment.NewLine +
                                "I made a private channel for you, please use `$auth [token]` to link this Discord server with your Fork app." +
                                Environment.NewLine +
                                "You can check for your token in Fork app settings.");
                        //var ownerr = KKK.Client.GetGuild(guild.Id).OwnerId;
                        await vChan.SendMessageAsync($"<@{guild.OwnerId}>", false, ebd.Build());
                            var msgg = await vChan.SendMessageAsync(null, false,
                                BotTools.Embed("Don't remove this message, this message will be updated continuously and display the status of you Fork servers.", 20));
                               using (var contextt = new DatabaseContext())
                        {
                            OnJoin newset = new OnJoin();
                                newset.Serverid = guild.Id;
                                newset.Roleid = roleee.Id;
                                newset.Channelid = vChan.Id;
                                newset.Messageid = msgg.Id;
                                contextt.Add(newset);
                                contextt.SaveChanges();
                        }

                        }
                        else
                        {
                            var ebd = new EmbedBuilder();
                            ebd.Color = Color.Red;
                            ebd.WithCurrentTimestamp();
                            ebd.WithAuthor($"Error", guild.CurrentUser.GetAvatarUrl());
                            ebd.WithDescription(warning);
                        //var ownerr = KKK.Client.GetGuild(guild.Id).OwnerId;
                        await guild.DefaultChannel.SendMessageAsync($"<@{guild.OwnerId}>", false, ebd.Build());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });
        }

        private async Task OnGuildLeave(SocketGuild guild)
        {
            await Task.Run(async () =>
            {
                try
                {
                    DatabaseContext context = new DatabaseContext();
                    if ((context.Auth.Any(o => o.Serverid == guild.Id) || ((context.OnJoin.Any(o => o.Serverid == guild.Id)))))

                    {
                        try
                        {
                             string token = context.Auth.AsQueryable().Where(a => a.Serverid == guild.Id).Single().Token;
                            await BotTools.Sendmsg(guild.Id, $"status|OnHold");
                            await BotTools.Sendmsg(guild.Id, $"rec");
                            await BotTools.Sendmsg(guild.Id, $"unsub|serverListEvent");
                            await BotTools.Sendmsg(guild.Id, $"unsub|playerEvent");
                            Program.AliveTokens.Remove(token);
                        }
                        catch (Exception ex)
                        {
                           logger.Warn($"Exception occured: {ex.ToString()}");
                        }
                         context.Remove(context.Auth.Single(a => a.Serverid == guild.Id));
                        context.Remove(context.Notify.Single(a => a.Serverid == guild.Id));
                        context.Remove(context.OnJoin.Single(a => a.Serverid == guild.Id));
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });
        }



        private async Task OnClientDisconnect(Exception k)
        {
            KKK.IsClientReady = false;
        }

        private async Task OnClientReady()
        {
            KKK.IsClientReady = true;
            try
            {
                await KKK.Client.SetGameAsync($"your Minecraft servers", null, ActivityType.Watching);
            }
            catch (Exception ex)
            {
               logger.Warn($"Exception occured: {ex.ToString()}");
            }
        }

        private async Task welcome(SocketGuildUser user)
        {
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            _ = Task.Run(async () =>
                        {

                            SocketUserMessage userMessage = message as SocketUserMessage;
                            if (userMessage is null || userMessage.Author.IsBot || userMessage.Author.IsWebhook)
                                return;
                            int argPos = 0;
                            var context = new SocketCommandContext(KKK.Client, userMessage);
                            try
                            {
                                if (message.Channel.GetType().ToString() != "Discord.WebSocket.SocketDMChannel")
                                {
                                    if (!userMessage.HasMentionPrefix(KKK.Client.CurrentUser, ref argPos) &&
                                        !userMessage.HasStringPrefix(KKK.prefix, ref argPos, StringComparison.OrdinalIgnoreCase))
                                    {
                                        return;
                                    }
                                    DatabaseContext contextt = new DatabaseContext();

                                    var authorr = context.Guild.GetUser(context.Message.Author.Id);
                                    var thisss = context.Message.Author as SocketGuildUser;
                                    ulong roleeid = (contextt.OnJoin.AsQueryable().Where(a => a.Serverid == context.Guild.Id).FirstOrDefault() ==null) ? 0 : contextt.OnJoin.AsQueryable().Where(a => a.Serverid == context.Guild.Id).FirstOrDefault().Roleid;


                            
                                    if (authorr.Roles.Any(r => r.Id == roleeid) || thisss.GuildPermissions.ManageGuild)
                                    {
                                        if (userMessage.Content.ToLower().StartsWith($"{KKK.prefix}auth") ||
                                            userMessage.Content.ToLower().StartsWith($"{KKK.prefix}leave")
                                           ) //if its for authentication let the command to be executed
                                        {
                                            string command = userMessage.Content.Substring(argPos).Trim();
                                            var result = await KKK.CommandService.ExecuteAsync(context, command, Services);

                                            if (!result.IsSuccess)
                                            {
                                                if ((int?)result.Error != (int?)CommandError.UnknownCommand)
                                                {
                                                    Console.WriteLine(result.ErrorReason);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    await context.Channel.SendMessageAsync($"Im Not Working yet", false, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Warn($"Exception occured: {ex.ToString()}");
                            }
                        });
      
        }
    }
}