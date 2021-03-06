﻿using System;
using UnityEngine;
using Adrenak.Unex;
using RestSharp;

namespace Adrenak.UniMap {
	public class PanoDownloader {
		/// <summary>
		/// Invoked everytime a request is finished. Null if the request was not successful
		/// </summary>
		public event Action<Texture32> OnLoaded;
		public event Action<Exception> OnFailed;
		public event Action OnStarted;

		GooglePanoDownloader m_GoogleDownloader;
		UserPanoDownloader m_UserDownloader;

		public PanoDownloader() {
			m_UserDownloader = new UserPanoDownloader();
			m_GoogleDownloader = new GooglePanoDownloader();

			m_UserDownloader.OnStarted += () => OnStarted.TryInvoke();
			m_UserDownloader.OnLoaded += tex => OnLoaded.TryInvoke(tex);
			m_UserDownloader.OnFailed += exception => OnFailed.TryInvoke(exception);

			m_GoogleDownloader.OnStarted += () => OnStarted.TryInvoke();
			m_GoogleDownloader.OnLoaded += tex => OnLoaded.TryInvoke(tex);
			m_GoogleDownloader.OnFailed += exception => OnFailed.TryInvoke(exception);
		}

		/// <summary>
		/// Downloads the panorama texture as a Promise
		/// </summary>
		/// <param name="panoID">The ID of the panorama to be downloaded</param>
		/// <param name="size">Size of the texture to be downloaded</param>
		public IPromise<Texture32> Download(string panoID, PanoSize size, TextureFormat format) {
			var promise = new Promise<Texture32>();
			Download(panoID, size, format,
				texture => promise.Resolve(texture),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		/// <summary>
		/// Downloads the panorama texture.
		/// </summary>
		/// <param name="panoID">The ID of the panorama to be downloaded</param>
		/// <param name="size">Size of the texture to be downloaded</param>
		/// <param name="onResult">Callback when the download is successful</param>
		/// <param name="onException">Callback when the download is unsuccessful</param>
		public void Download(string panoID, PanoSize size, TextureFormat format, Action<Texture32> onResult, Action<Exception> onException) {
			// We first try to download as a Google Pano
			m_GoogleDownloader.IsAvailable(panoID)
				.Then(isGooglePano => {
					if (isGooglePano)
						GoogleDownload(panoID, size, format, onResult, onException);
					else {
						m_UserDownloader.IsAvailable(panoID)
							.Then(isUserPano => {
								if (isUserPano)
									UserDownload(panoID, size, format, onResult, onException);
								else {
									onException.TryInvoke(new Exception("Pano cannot be downloaded!"));
									Stop();
								}
							});
					}
				});
		}

		/// <summary>
		/// Stops downloading the panorama
		/// </summary>
		public void Stop() {
			if (m_GoogleDownloader != null)
				m_GoogleDownloader.Stop();
			if (m_UserDownloader != null)
				m_UserDownloader.Stop();
		}

		void GoogleDownload(string panoID, PanoSize size, TextureFormat format, Action<Texture32> onResult, Action<Exception> onException) {
			if (m_UserDownloader != null) {
				m_UserDownloader.ClearTexture();
				m_UserDownloader.Stop();
			}
			m_GoogleDownloader.Download(panoID, size, format, onResult, onException);
		}

		void UserDownload(string panoID, PanoSize size, TextureFormat format, Action<Texture32> onResult, Action<Exception> onException) {
			if (m_GoogleDownloader != null) {
				m_GoogleDownloader.ClearTexture();
				m_GoogleDownloader.Stop();
			}
			m_UserDownloader.Download(panoID, size, format, onResult, onException);
		}
	}
}
