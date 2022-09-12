using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.HandBook;
using RomDiscord.Services;
using System.Collections.Generic;
using System.Linq;

namespace RomDiscord.DiscordModules
{
	public class HandbookModule : ModuleBase<SocketCommandContext>
	{
		IServiceProvider services;

		public HandbookModule(IServiceProvider services)
		{
			this.services = services;
		}

		[Command("ImageUpload")]
		public async Task ImageUpload()
		{
			if (Context.Message.Attachments.Count == 0)
				return;
			using var scope = services.CreateScope();
			using var context = scope.ServiceProvider.GetRequiredService<Context>();
			var moduleSettings = scope.ServiceProvider.GetRequiredService<ModuleSettings>();
            var handbookService = scope.ServiceProvider.GetRequiredService<HandbookService>();
            if (Context.Guild != null)
            {
                var guild = context.Guilds.FirstOrDefault(g => g.DiscordGuildId == Context.Guild.Id);
                if (guild != null)
                {
                    var settings = new SettingsModel(moduleSettings, guild);
                    if (!settings.Enabled || settings.Channel != Context.Channel.Id)
                        return;
                }
            }

            var results = new List<HandbookState>();
            foreach (var attachment in Context.Message.Attachments)
            {
                var extension = Path.GetExtension(attachment.Filename).ToLower();
                if (extension != ".jpg" && extension != ".png")
                {
                    Console.WriteLine("Invalid image");
                    continue;
                }
                HttpClient client = new HttpClient();
                var stream = await (await client.GetAsync(attachment.Url)).Content.ReadAsStreamAsync();
                try
                {
                    string error = "";
                    results.AddRange(handbookService.ScanPicture(stream, Context.User.Id, ref error));
                    if(error != "")
                        await ReplyAsync("Error reading your picture: " + error);

                    if (results == null)
                        await ReplyAsync("Error reading your picture. please try again with a different one");
                }
                catch (Exception e)
                {
                    await ReplyAsync("Error reading your picture. please try again with a different one: " + e.InnerException.Message);
                    break;
                }
            }
            try
            {
                await Context.Message.DeleteAsync();
            }
            catch (Exception) { }
            var eb = new EmbedBuilder()
                .WithTitle("Image Parse Results");
            foreach (var r in results)
            {
                try
                {
                    eb.AddField(r.Stat.ToString(), r.Value + "", true);
                    context.HandbookStates.Add(r);
                    await context.SaveChangesAsync();
                }
                catch (Exception) { }
            }
            await ReplyAsync(null, false, eb.Build());
        }

    }
}