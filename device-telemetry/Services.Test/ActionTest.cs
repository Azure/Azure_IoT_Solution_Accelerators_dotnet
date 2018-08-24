// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Newtonsoft.Json.Linq;
using Services.Test.helpers;
using Xunit;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;

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


        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionModel_When_ValidActionType()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                {PARAM_SUBJECT_KEY, PARAM_SUBJECT},
                {PARAM_TEMPLATE_KEY, PARAM_TEMPLATE},
                {PARAM_EMAIL_KEY, new JArray() { PARAM_EMAIL} }
            };

            // Act 
            var targetAction = new EmailActionItem(Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email, parameters);

            // Assert 
            Assert.True(this.IsEmailActionItemReadProperly(targetAction));
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
            Assert.Throws<InvalidInputException>(() => new EmailActionItem(Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email, parameters));
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
            Assert.Throws<InvalidInputException>(() => new EmailActionItem(Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email, parameters));
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
            Assert.Throws<InvalidInputException>(() => new EmailActionItem(Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email, parameters));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnActionModel_When_ValidActionTypeParametersIsCaseInsensitive()
        {
            // Arrange
            var parameters = new Dictionary<string, object>()
            {
                {PARAM_SUBJECT_KEY, PARAM_SUBJECT},
                {"tEmPlate", PARAM_TEMPLATE},
                {"eMail", new JArray() { PARAM_EMAIL} }
            };

            // Act 
            var targetAction = new EmailActionItem(Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email, parameters);

            // Assert 
            Assert.True(this.IsEmailActionItemReadProperly(targetAction));
        }

        private bool IsEmailActionItemReadProperly(EmailActionItem emailActionItem)
        {
            return emailActionItem.Type == Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Type.Email
                && string.Equals(emailActionItem.Parameters[PARAM_TEMPLATE_KEY], PARAM_TEMPLATE)
                && this.IsListOfEmailEqual((List<string>)emailActionItem.Parameters[PARAM_EMAIL_KEY]);
        }

        private bool IsListOfEmailEqual(IList<string> emailList)
        {
            var checkList = new JArray() { PARAM_EMAIL };
            foreach (var email in checkList)
            {
                if (!emailList.Contains((string)email)) return false;
            }
            return true;
        }
    }
}
