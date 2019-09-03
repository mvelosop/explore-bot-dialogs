using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComponentDialogs.Bot.Core
{
    public class ComponentDialogsBotAccessors
    {
        private readonly Dictionary<string, object> _stateAccessors = new Dictionary<string, object>();

        public ComponentDialogsBotAccessors(
            ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            DialogState = GetAccessor<DialogState>();
        }

        public ConversationState ConversationState { get; }

        public IStatePropertyAccessor<DialogState> DialogState { get; }

        public async Task<TState> GetAsync<TState>(ITurnContext context, CancellationToken cancellationToken)
            where TState : new()
        {
            var accessor = GetAccessor<TState>();

            return await accessor.GetAsync(context, () => new TState(), cancellationToken);
        }

        public async Task<TState> SetAsync<TState>(ITurnContext context, Action<TState> updateAction, CancellationToken cancellationToken)
            where TState : new()
        {
            var accessor = GetAccessor<TState>();
            var state = await accessor.GetAsync(context, () => new TState(), cancellationToken);

            updateAction.Invoke(state);

            await accessor.SetAsync(context, state, cancellationToken);

            return state;
        }

        private IStatePropertyAccessor<TState> GetAccessor<TState>()
        {
            var accessorKey = $"{nameof(ComponentDialogsBotAccessors)}.{typeof(TState).Name}";
            IStatePropertyAccessor<TState> accessor = null;

            if (_stateAccessors.ContainsKey(accessorKey))
            {
                accessor = _stateAccessors[accessorKey] as IStatePropertyAccessor<TState>;
            }
            else
            {
                accessor = ConversationState.CreateProperty<TState>(accessorKey);
                _stateAccessors.Add(accessorKey, accessor);
            }

            return accessor;
        }
    }
}