namespace ConferenceBot.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    public class ConferenceBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;

        public ConferenceBot(ConversationState conversationState, UserState userState, T dialog, ILogger<ConferenceBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext tc, CancellationToken ct = default)
        {
            await base.OnTurnAsync(tc, ct);
            await ConversationState.SaveChangesAsync(tc, false, ct);
            await UserState.SaveChangesAsync(tc, false, ct);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> tc, CancellationToken ct)
        {
            Logger.LogInformation("Running dialog with Message Activity.");
            await Dialog.RunAsync(tc, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), ct);
        }
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> ca, ITurnContext<IConversationUpdateActivity> tc, CancellationToken ct)
        {
            var welcomeCard = CreateAdaptiveCardAttachment();
            var welcomeResponse = MessageFactory.Attachment(welcomeCard, ssml: "Welcome to Conference Registration!");
            foreach (var member in ca)
            {
                if (member.Id != tc.Activity.Recipient.Id)
                {
                    await tc.SendActivityAsync(welcomeResponse, ct);
                }
            }
        }
        /// <summary>  
        /// Adaptive Card for welcoming user
        /// </summary>  
        /// <returns></returns>  
        private static Attachment CreateAdaptiveCardAttachment()
        {
            AdaptiveCard weclomeCard = new AdaptiveCard();

            weclomeCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Conference Registration",
                Size = AdaptiveTextSize.Large,
                Wrap = true
            });
            weclomeCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = "30th April 2023, Pune",
                Size = AdaptiveTextSize.Medium
            });

            weclomeCard.Body.Add(new AdaptiveImage()
            {
                Url = new Uri("data:image/jpeg;base64,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"),
                //Url = new Uri("image URI"),
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center
            });
            weclomeCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = " Hi, data enthusiast.",
                Size = AdaptiveTextSize.Small,
                Wrap = true,
                Weight = AdaptiveTextWeight.Bolder

            });
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = weclomeCard
            };
            return attachment;
        }
    }
}
