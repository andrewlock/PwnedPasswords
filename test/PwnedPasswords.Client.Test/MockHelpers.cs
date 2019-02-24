// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace PwnedPasswords.Client.Test
{
    public static class MockHelpers
    {
        public static StringBuilder LogMessage = new StringBuilder();

        public static Mock<ILogger<T>> MockILogger<T>(StringBuilder logStore = null) where T : class
        {
            logStore = logStore ?? LogMessage;
            var logger = new Mock<ILogger<T>>();
            logger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(),
                It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
                .Callback((LogLevel logLevel, EventId eventId, object state, Exception exception, Func<object, Exception, string> formatter) =>
                {
                    if (formatter == null)
                    {
                        logStore.Append(state.ToString());
                    }
                    else
                    {
                        logStore.Append(formatter(state, exception));
                    }
                    logStore.Append(" ");
                });
            logger.Setup(x => x.BeginScope(It.IsAny<object>())).Callback((object state) =>
                {
                    logStore.Append(state.ToString());
                    logStore.Append(" ");
                });
            logger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            logger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);

            return logger;
        }

        public static ILogger<T> StubLogger<T>()
        {
            var stub = new Mock<ILogger<T>>();
            return stub.Object;
        }

        public static IOptions<T> Options<T>(T options) where T : class, new()
        {
            var stub = new Mock<IOptions<T>>();
            stub.SetupGet(x => x.Value).Returns(options);
            return stub.Object;
        }

        public static IOptions<T> Options<T>() where T : class, new()
        {
            return Options(new T());
        }
    }
}