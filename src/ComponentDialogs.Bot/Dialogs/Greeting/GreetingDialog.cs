using ComponentDialogs.Bot.Dialogs.Greeting;
using ComponentDialogs.Bot.Core;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using Registration.Application.Contracts;
using Registration.Domain.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComponentDialogs.Bot.Dialogs.Greeting
{
    public class GreetingDialog : ComponentDialog
    {
        public const string GreetingDialogId = nameof(GreetingDialogId);
        public const string TextPromptId = nameof(TextPromptId);

        private readonly ComponentDialogsBotAccessors _accessors;
        private readonly IBotUserServices _botUserServices;
        private readonly ILogger<GreetingDialog> _logger;

        public GreetingDialog(
            ILogger<GreetingDialog> logger,
            ComponentDialogsBotAccessors accessors,
            IBotUserServices botUserServices)
            : base(GreetingDialogId)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _botUserServices = botUserServices ?? throw new ArgumentNullException(nameof(botUserServices));

            InitialDialogId = Id;

            AddDialog(new WaterfallDialog(GreetingDialogId)
                .AddStep(Step1CheckRegistrationAsync)
                .AddStep(Step2GetCallNameAsync)
                .AddStep(Step3ThankYouAsync));

            AddDialog(new TextPrompt(TextPromptId));
        }

        private BotUser GetBotUser(WaterfallStepContext stepContext)
        {
            return (BotUser)stepContext.Values[nameof(BotUser)];
        }

        private async Task<DialogTurnResult> Step1CheckRegistrationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("----- ComponentDialogBot.Step1CheckRegistrationAsync - Beginning step");

            var context = stepContext.Context;

            // Check if user is already registered
            var user = await _botUserServices.FindByChannelUserIdAsync(context.Activity.ChannelId, context.Activity.From.Name);

            if (user != null)
            {
                await _accessors.SetAsync<GreetingState>(stepContext.Context, s => s.CallName = user.CallName, cancellationToken);

                // Get GreetingState from conversation state
                await context.SendActivityAsync($"Hi {user.CallName}, nice to talk to you again!");

                return await stepContext.EndDialogAsync();
            }

            await context.SendActivityAsync($"Hi {context.Activity.From.Name}! You are not registered in our database.");

            var botUser = new BotUser
            {
                ChannelId = context.Activity.ChannelId,
                UserId = context.Activity.From.Name,
            };

            StoreBotUser(stepContext, botUser);

            return await stepContext.PromptAsync(
                TextPromptId,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your name") },
                cancellationToken);
        }

        private async Task<DialogTurnResult> Step2GetCallNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("----- ComponentDialogBot.Step2GetCallNameAsync - Beginning step");

            var botUser = GetBotUser(stepContext);
            botUser.Name = (string)stepContext.Result;

            return await stepContext.PromptAsync(
                TextPromptId,
                new PromptOptions { Prompt = MessageFactory.Text($"Thanks {botUser.Name}, How do you want me to call you?") },
                cancellationToken);
        }

        private async Task<DialogTurnResult> Step3ThankYouAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogTrace("----- ComponentDialogBot.Step3ThankYouAsync - Beginning step");

            var botUser = GetBotUser(stepContext);

            botUser.CallName = (string)stepContext.Result;

            await _botUserServices.AddAsync(botUser);

            _logger.LogTrace("----- ComponentDialogBot.Step3ThankYouAsync - Checking users in database: {@Users}", await _botUserServices.GetListAsync());

            await _accessors.SetAsync<GreetingState>(stepContext.Context, s => s.CallName = botUser.CallName, cancellationToken);

            await stepContext.Context.SendActivityAsync($"Thanks {botUser.CallName}, I'll echo you from now on, just type anything", cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private void StoreBotUser(WaterfallStepContext stepContext, BotUser botUser)
        {
            stepContext.Values[nameof(BotUser)] = botUser;
        }
    }
}