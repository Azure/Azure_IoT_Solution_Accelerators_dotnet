// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json.Linq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class ActionTest
    {
        private const string PARAM_TEMPLATE = "Chiller pressure is at 250 which is high";
        private const string PARAM_SUBJECT = "Alert Notification";
        private const string PARAM_EMAIL = "sampleEmail@gmail.com";
        private const string PARAM_SUBJECT_KEY = "Subject";
        private const string PARAM_TEMPLATE_KEY = "Template";
        private const string PARAM_EMAIL_KEY = "Email";

        private readonly JArray emailArray;

        public ActionTest()
        {
            this.emailArray = new JArray { PARAM_EMAIL };
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionModel_When_ValidActionType()
        {
            // Arrange
            var parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { PARAM_SUBJECT_KEY, PARAM_SUBJECT },
                { PARAM_TEMPLATE_KEY, PARAM_TEMPLATE },
                { PARAM_EMAIL_KEY, this.emailArray }
            };

            // Act 
            var result = new EmailActionItem(parameters);

            // Assert 
            Assert.Equal(ActionType.Email, result.ActionType);
            Assert.Equal(PARAM_TEMPLATE, result.Parameters[PARAM_TEMPLATE_KEY]);
            Assert.Equal(this.emailArray, result.Parameters[PARAM_EMAIL_KEY]);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeIsEmailAndInvalidEmail()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                {PARAM_SUBJECT_KEY, PARAM_SUBJECT},
                {PARAM_TEMPLATE_KEY, PARAM_TEMPLATE},
                {PARAM_EMAIL_KEY, new Newtonsoft.Json.Linq.JArray() { "sampleEmailgmail.com"} }
            };

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => new EmailActionItem(parameters));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_Throw_InvalidInputException_WhenActionTypeIsEmailAndNoEmailField()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                {PARAM_SUBJECT_KEY, PARAM_SUBJECT},
                {PARAM_TEMPLATE_KEY, PARAM_TEMPLATE}
            };

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => new EmailActionItem(parameters));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowInvalidInputException_When_ActionTypeIsEmailAndEmailIsString()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                {PARAM_SUBJECT_KEY, PARAM_SUBJECT},
                {PARAM_TEMPLATE_KEY, PARAM_TEMPLATE},
                {PARAM_EMAIL_KEY, PARAM_EMAIL}
            };

            // Act and Assert
            Assert.Throws<InvalidInputException>(() => new EmailActionItem(parameters));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionModel_When_ValidActionTypeParametersIsCaseInsensitive()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                { PARAM_SUBJECT_KEY, PARAM_SUBJECT },
                { "tEmPlate", PARAM_TEMPLATE },
                { "eMail", this.emailArray }
            };

            // Act 
            var result = new EmailActionItem(parameters);

            // Assert 
            Assert.Equal(ActionType.Email, result.ActionType);
            Assert.Equal(PARAM_TEMPLATE, result.Parameters[PARAM_TEMPLATE_KEY]);
            Assert.Equal(this.emailArray, result.Parameters[PARAM_EMAIL_KEY]);
        }
    }
}
