﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using static Microsoft.Bot.Builder.Prompts.PromptValidatorEx;

namespace Microsoft.Bot.Builder.Prompts
{
    public class NumberResult<T> : RecognitionResult
    {
        public T Value { get; set; }

        public string Text { get; set; }
    }

    /// <summary>
    /// NumberPrompt recognizes floats or ints 
    /// </summary>
    public class NumberPrompt<T> : BasePrompt<NumberResult<T>>
    {
        private IModel _model;

        public NumberPrompt(string culture, PromptValidator<NumberResult<T>> validator = null)
            : base(validator)
        {
            _model = NumberRecognizer.Instance.GetNumberModel(culture);
        }

        protected NumberPrompt(IModel model, PromptValidator<NumberResult<T>> validator = null)
            : base(validator)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Used to validate the incoming text, expected on context.Request, is
        /// valid according to the rules defined in the validation steps. 
        /// </summary>        
        public override async Task<NumberResult<T>> Recognize(IBotContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Request);
            if (context.Request.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");

            NumberResult<T> numberResult = new NumberResult<T>();

            IMessageActivity message = context.Request.AsMessageActivity();
            var results = _model.Parse(message.Text);
            if (results.Any())
            {
                var result = results.First();
                if (typeof(T) == typeof(float))
                {
                    if (float.TryParse(result.Resolution["value"].ToString(), out float value))
                    {
                        numberResult.Status = RecognitionStatus.Recognized;
                        numberResult.Value = (T)(object)value;
                        numberResult.Text = result.Text;
                        await Validate(context, numberResult);
                    }
                }
                else if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(result.Resolution["value"].ToString(), out int value))
                    {
                        numberResult.Status = RecognitionStatus.Recognized;
                        numberResult.Value = (T)(object)value;
                        numberResult.Text = result.Text;
                        await Validate(context, numberResult);
                    }
                }
            }
            return numberResult;
        }
    }
}
