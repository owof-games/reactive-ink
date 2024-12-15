using System.Collections.Generic;
using Ink.Runtime;

namespace ReactiveInk
{
    public readonly struct StoryChoice
    {
        private readonly Choice _choice;

        public string Text => _choice.text.Trim();

        public int Index => _choice.index;

        public IEnumerable<string> Tags => _choice.tags;

        public StoryChoice(Choice choice)
        {
            _choice = choice;
        }
    }
}