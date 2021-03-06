﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChatdollKit.Dialog
{
    public class DialogRouter
    {
        private Dictionary<string, IDialogProcessor> intentResolver;
        private Dictionary<string, IDialogProcessor> topicResolver;

        public DialogRouter()
        {
            intentResolver = new Dictionary<string, IDialogProcessor>();
            topicResolver = new Dictionary<string, IDialogProcessor>();
        }

        public IDialogProcessor Route(Request request, Context context, CancellationToken token)
        {
            // Update topic
            IDialogProcessor dialogProcessor;
            if (intentResolver.ContainsKey(request.Intent) && (request.IntentPriority > context.Topic.Priority || string.IsNullOrEmpty(context.Topic.Name))){
                dialogProcessor = intentResolver[request.Intent];
                if (!request.IsAdhoc)
                {
                    context.Topic.Name = dialogProcessor.TopicName;
                    context.Topic.Status = "";
                    if (request.IntentPriority >= Priority.Highest)
                    {
                        // Set slightly lower priority to enable to update Highest priority intent
                        context.Topic.Priority = Priority.Highest - 1;
                    }
                    else
                    {
                        context.Topic.Priority = request.IntentPriority;
                    }
                    context.Topic.IsNew = true;
                }
                else
                {
                    // Do not update topic when request is adhoc
                    if (!string.IsNullOrEmpty(context.Topic.Name))
                    {
                        context.Topic.ContinueTopic = true;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(context.Topic.Name))
            {
                // Continue topic
                dialogProcessor = topicResolver[context.Topic.Name];
            }
            else
            {
                // Use default when the intent is not determined
                throw new Exception("No dialog processor found");
            }

            return dialogProcessor;
        }

        public void RegisterIntent(string intentName, IDialogProcessor dialogProcessor)
        {
            dialogProcessor.Configure();
            intentResolver.Add(intentName, dialogProcessor);
            topicResolver.Add(dialogProcessor.TopicName, dialogProcessor);
        }
    }

}
