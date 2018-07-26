// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class AuthenticationMechanismServiceModel
    {
        public AuthenticationMechanismServiceModel()
        {
        }

        internal AuthenticationMechanismServiceModel(AuthenticationMechanism azureModel)
        {
            switch (azureModel.Type)
            {
                case Azure.Devices.AuthenticationType.Sas:
                    this.PrimaryKey = azureModel.SymmetricKey.PrimaryKey;
                    this.SecondaryKey = azureModel.SymmetricKey.SecondaryKey;
                    break;
                case Azure.Devices.AuthenticationType.SelfSigned:
                    this.AuthenticationType = AuthenticationType.SelfSigned;
                    this.PrimaryThumbprint = azureModel.X509Thumbprint.PrimaryThumbprint;
                    this.SecondaryThumbprint = azureModel.X509Thumbprint.SecondaryThumbprint;
                    break;
                case Azure.Devices.AuthenticationType.CertificateAuthority:
                    this.AuthenticationType = AuthenticationType.CertificateAuthority;
                    this.PrimaryThumbprint = azureModel.X509Thumbprint.PrimaryThumbprint;
                    this.SecondaryThumbprint = azureModel.X509Thumbprint.SecondaryThumbprint;
                    break;
                default:
                    throw new ArgumentException("Not supported authentcation type");
            }
        }

        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }

        public string PrimaryThumbprint { get; set; }

        public string SecondaryThumbprint { get; set; }

        public AuthenticationType AuthenticationType { get; set; }

        public AuthenticationMechanism ToAzureModel()
        {
            var auth = new AuthenticationMechanism();

            switch (this.AuthenticationType)
            {
                case AuthenticationType.Sas:
                {
                    auth.SymmetricKey = new SymmetricKey()
                    {
                        PrimaryKey = this.PrimaryKey,
                        SecondaryKey = this.SecondaryKey
                    };

                    auth.Type = Azure.Devices.AuthenticationType.Sas;

                    break;
                }
                case AuthenticationType.SelfSigned:
                {
                    auth.X509Thumbprint = new X509Thumbprint()
                    {
                        PrimaryThumbprint = this.PrimaryThumbprint,
                        SecondaryThumbprint = this.SecondaryThumbprint
                    };

                    auth.Type = Azure.Devices.AuthenticationType.SelfSigned;

                    break;
                }
                case AuthenticationType.CertificateAuthority:
                {
                    auth.X509Thumbprint = new X509Thumbprint()
                    {
                        PrimaryThumbprint = this.PrimaryThumbprint,
                        SecondaryThumbprint = this.SecondaryThumbprint
                    };

                    auth.Type = Azure.Devices.AuthenticationType.CertificateAuthority;

                    break;
                }
                default:
                    throw new ArgumentException("Not supported authentcation type");
            }

            return auth;
        }
    }

    public enum AuthenticationType
    {
        //
        // Summary:
        //     Shared Access Key
        Sas = 0,

        //
        // Summary:
        //     Self-signed certificate
        SelfSigned = 1,

        //
        // Summary:
        //     Certificate Authority
        CertificateAuthority = 2
    }
}
