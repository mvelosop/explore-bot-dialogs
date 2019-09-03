// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using ComponentDialogBot.Dialogs.Greeting;
using ComponentDialogs.Bot.Dialogs.Greeting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComponentDialogsBot.Bots
{
    public class DialogBot : ActivityHandler
    {
        private readonly ComponentDialogsBotAccessors _accessors;
        private readonly GreetingDialog _greetingDialog;
        private readonly ILogger<DialogBot> _logger;

        public DialogBot(
            ILogger<DialogBot> logger,
            ComponentDialogsBotAccessors accessors,
            GreetingDialog greetingDialog)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
            _greetingDialog = greetingDialog ?? throw new System.ArgumentNullException(nameof(greetingDialog));

            Dialogs = new DialogSet(_accessors.DialogState)
                .Add(greetingDialog);

            _logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
        }

        public DialogSet Dialogs { get; }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _accessors.ConversationState.SaveChangesAsync(turnContext);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello world!"), cancellationToken);
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Create the dialog context
            var dialogContext = await Dialogs.CreateContextAsync(turnContext, cancellationToken);

            _logger.LogTrace("----- ComponentDialogsBot - ActiveDialog: {ActiveDialog} - DialogStack: {@DialogStack}", dialogContext.ActiveDialog?.Id, dialogContext.Stack);

            if (dialogContext.ActiveDialog != null)
            {
                var dialogResult = await dialogContext.ContinueDialogAsync(cancellationToken);

                return;
            }

            var greetingState = await _accessors.GetAsync<GreetingState>(turnContext, cancellationToken);

            if (greetingState.CallName == null)
            {
                await dialogContext.BeginDialogAsync(GreetingDialog.GreetingDialogId, null, cancellationToken);

                return;
            }

            // Set the conversation state from the turn context.
            var state = await _accessors.SetAsync<CounterState>(turnContext, s => s.TurnCount++, cancellationToken);

            // Echo back to the user whatever they typed.
            var responseMessage = $"Hi {greetingState.CallName} (Turn {state.TurnCount}): You typed \"{turnContext.Activity.Text}\"";

            await turnContext.SendActivityAsync(responseMessage);
        }
    }
}