using Discord;
using Discord.Commands;
using Discord.Interactions;

namespace RomDiscord.DiscordModules
{
	public class TestModule : InteractionModuleBase<SocketInteractionContext>
	{
		IServiceProvider services;
		public TestModule(IServiceProvider services)
		{
			this.services = services;
		}

        [SlashCommand("clearmessages", "Clears all the messages on a channel")]
        public async Task ClearMessages()
        {
            await this.RespondAsync("Clearing....");

            var messages = await this.Context.Channel.GetMessagesAsync(100).FlattenAsync();
            foreach(var message in messages)
            {
                try
                {
                    await message.DeleteAsync();
                    await Task.Delay(500);
                }
                catch (Exception) { }
            }
        }

        /*		[SlashCommand("ping", "Pings the bot")]
                public Task PingAsync()
                    => Context.Interaction.RespondAsync("pong!");

                // Defines the modal that will be sent.
                public class FoodModal : IModal
                {
                    public string Title => "Fav Food";
                    // Strings with the ModalTextInput attribute will automatically become components.
                    [InputLabel("What??")]
                    [ModalTextInput("food_name", placeholder: "Pizza", maxLength: 20)]

                    public string Food { get; set; } = null!;

                    // Additional paremeters can be specified to further customize the input.
                    [InputLabel("Why??")]
                    [ModalTextInput("food_reason", TextInputStyle.Paragraph, "Kuz it's tasty", maxLength: 500)]
                    public string Reason { get; set; } = null!;
                }

                [SlashCommand("taskdebug", "Debug for borf")]
                public async Task TaskDebug()
                {
                    using var scope = services.CreateScope();
                    var scheduler = (RomDiscord.Services.TaskScheduler)services.GetServices<IHostedService>().First(s => (s as RomDiscord.Services.TaskScheduler) != null);
                    string msg = string.Join("\n", scheduler.tasks.Select(t => t.NextRun + " -> " + t.Name));
                    await RespondAsync(msg);
                }

                [SlashCommand("modal", "Shows a modal")]
                [EnabledInDm(false)]
                public async Task ModalAsync()
                {
                    await RespondWithModalAsync<FoodModal>("food_menu");
                }
                [SlashCommand("testembed", "Test stuff")]
                public async Task TestEmbed()
                {
                    var eb = new EmbedBuilder();
                    for(int i = 0; i < 1; i++)
                    {
                        string value = string.Join("\n", Enumerable.Repeat("MVP 1 killed by borf", 40));
                        eb.AddField(new EmbedFieldBuilder().WithName("Channel " + i).WithValue(value).WithIsInline(true));
                    }
                    await RespondAsync(null, new Embed[] { eb.Build() } );
                }

                [SlashCommand("test", "Test stuff")]
                public async Task TestAsync()
                {
                    SelectMenuBuilder smb = new SelectMenuBuilder();
                    smb.WithCustomId("Instance");
                    smb.AddOption("TTL", "TTL", "Thanatos Legend");
                    smb.AddOption("PML", "PML");
                    smb.AddOption("PMB", "PMB");
                    await RespondAsync("Yay", components: new ComponentBuilder().WithSelectMenu(smb).Build());
                }
                [ComponentInteraction("Instance")]
                public async Task TestSelection(string[] selected)
                {
                    await RespondAsync("omg " + selected[0]);
                }

                [SlashCommand("adduser", "Test")]
                public async Task AddUser(IUser user, string bla)
                {
                    await RespondAsync("omg " + user.Username);
                }
                // Responds to the modal.
                [ModalInteraction("food_menu")]
                public async Task ModalResponce(FoodModal modal)
                {
                    // Build the message to send.
                    string message = "hey @everyone, I just learned " +
                        $"{Context.User.Mention}'s favorite food is " +
                        $"{modal.Food} because {modal.Reason}.";

                    // Specify the AllowedMentions so we don't actually ping everyone.
                    AllowedMentions mentions = new();
                    mentions.AllowedTypes = AllowedMentionTypes.Users;

                    // Respond to the modal.
                    await RespondAsync(message, allowedMentions: mentions, ephemeral: true);
                }*/
    }
}