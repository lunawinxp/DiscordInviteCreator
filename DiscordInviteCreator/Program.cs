using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordInviteCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Luna's Server Invite Creator");
            Console.WriteLine("This tool will create permanant invites for all your servers your in");
            Console.WriteLine("And export them to 'discord-invites.txt'");
            Console.WriteLine();
            Console.WriteLine("!! WARNING, PLEASE READ !!");
            Console.WriteLine("!! WARNING, PLEASE READ !!");
            Console.WriteLine("!! WARNING, PLEASE READ !!");
            Console.WriteLine("Sending multiple requests from a user account can lead to bans, and I havent tested this");
            Console.WriteLine("By using this, Im not responsible if you get banned or anything similar.");
            Console.WriteLine("Press enter to acknowledge this warning");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine("Luna's Server Invite Creator");
            Console.Write("Enter your discord token: ");
            string token = Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Luna's Server Invite Creator");

            WebClient client = new WebClient();

            try
            {
                client.Headers.Add("Authorization", token);

                string json = client.DownloadString("https://discordapp.com/api/v8/users/@me/guilds");

                dynamic jsonobj = JArray.Parse(json);

                foreach (var itm in jsonobj)
                {
                    Console.WriteLine("Found server: " + itm.name);

                    string chnlJson = client.DownloadString("https://discordapp.com/api/v8/guilds/" + itm.id + "/channels");
                    dynamic chnljsonobj = JArray.Parse(chnlJson);

                    long id = 0;
                    string name = "";
                    foreach (var chnl in chnljsonobj)
                    {
                        if (chnl.type == 0)
                        {
                            Console.WriteLine("Found text channel: " + chnl.name + "! Using this for the invite...");
                            id = chnl.id;
                            name = chnl.name;
                            break;
                        }
                    }
                    if (id != 0)
                    {
                        try
                        {
                            client.Headers.Add("Content-Type", "application/json");
                            string invJson = client.UploadString("https://discordapp.com/api/v8/channels/" + id + "/invites", "{\"max_age\":0,\"max_uses\":0,\"temporary\":false}");
                            dynamic invjsonobj = JObject.Parse(invJson);
                            Console.WriteLine("Found created invite for: " + itm.name + "! discord.gg/" + invjsonobj.code);
                            File.AppendAllText("discord-invites.txt", "discord.gg/" + invjsonobj.code + " (" + itm.name + ", #" + name + ")\r\n");
                        }
                        catch
                        {
                            Console.WriteLine("Unable to create invites in " + itm.name + "... Checking for vanity urls...");
                            string guildResp = client.DownloadString("https://discordapp.com/api/v8/guilds/" + itm.id);
                            dynamic guildrespjson = JObject.Parse(guildResp);
                            try
                            {
                                if (((string)guildrespjson.vanity_url_code).Length > 0)
                                {
                                    Console.WriteLine("Found vanity url for: " + itm.name + "! discord.gg/" + guildrespjson.vanity_url_code);
                                    File.AppendAllText("discord-invites.txt", "discord.gg/" + guildrespjson.vanity_url_code + " (" + itm.name + ")\r\n");
                                }
                            }
                            catch
                            {

                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("Could not find any channels in " + itm.name + "...");
                    }

                    Thread.Sleep(3000); //wait 3 seconds just incase discord ratelimit, dont really want to risk banning people, if you want to try to make it faster, remove this
                }
                Console.Clear();
                Console.WriteLine("Done! Servers have been backed up to 'discord-invites.txt'");
            }
            catch
            {
                Console.WriteLine("Something went wrong, is your token correct?");
                Console.ReadLine();
            }
        }
    }
}