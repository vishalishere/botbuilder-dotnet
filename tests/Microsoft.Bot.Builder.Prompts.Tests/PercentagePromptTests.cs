﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Percentage Prompts")]
    public class PercentagePromptTests
    {
        [TestMethod]
        public async Task PercentagePrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
                {
                    var state = ConversationState<TestState>.Get(context);
                    var testPrompt = new PercentagePrompt(Culture.English);
                    if (!state.InPrompt)
                    {
                        state.InPrompt = true;
                        await testPrompt.Prompt(context, "Gimme:");
                    }
                    else
                    {
                        var percentResult = await testPrompt.Recognize(context);
                        if (percentResult.Succeeded())
                        {
                            Assert.IsTrue(percentResult.Value != float.NaN);
                            Assert.IsNotNull(percentResult.Text);
                            Assert.IsInstanceOfType(percentResult.Value, typeof(float));
                            context.Reply($"{percentResult.Value}");
                        }
                        else
                            context.Reply(RecognitionStatus.NotRecognized.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply(RecognitionStatus.NotRecognized.ToString())
                .Send("give me 5")
                    .AssertReply(RecognitionStatus.NotRecognized.ToString())
                .Send(" I would like forty five percent")
                    .AssertReply("45")
                .StartTest();
        }

        [TestMethod]
        public async Task PercentagePrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new PercentagePrompt(Culture.English, async (ctx, result) =>
                {
                    if (result.Value <= 10)
                        result.Status = RecognitionStatus.TooSmall;
                });
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await numberPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var percentResult = await numberPrompt.Recognize(context);
                    if (percentResult.Succeeded())
                        context.Reply($"{percentResult.Value}");
                    else
                        context.Reply(percentResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send(" I would like 5%")
                    .AssertReply(RecognitionStatus.TooSmall.ToString())
                .Send(" I would like 30%")
                    .AssertReply("30")
                .StartTest();
        }

    }
}