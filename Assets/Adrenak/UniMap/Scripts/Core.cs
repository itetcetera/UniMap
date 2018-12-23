﻿using System.Net;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System;

namespace Adrenak.UniMap {
	public static class Core {
		public static void Init() {
			ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => {
				bool isOk = true;
				// If there are errors in the certificate chain,
				// look at each error to determine the cause.
				if (sslPolicyErrors != SslPolicyErrors.None) {
					for (int i = 0; i < chain.ChainStatus.Length; i++) {
						if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown) {
							continue;
						}
						chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
						chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
						chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
						chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
						bool chainIsValid = chain.Build((X509Certificate2)certificate);
						if (!chainIsValid) {
							isOk = false;
							break;
						}
					}
				}
				return isOk;
			};
		}
	}
}