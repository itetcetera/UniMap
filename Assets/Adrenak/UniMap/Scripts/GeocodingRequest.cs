﻿using RestSharp;
using System.Text;
using System;
using Adrenak.Unex;
using UnityEngine;

namespace Adrenak.UniMap {
	/// <summary>
	/// Used to get the coordinates of an address
	/// </summary>
	public class GeocodingRequest {
		const string k_BaseURL = "https://maps.googleapis.com/maps/api/geocode/json?";
		/// <summary>
		/// The Google Maps API key
		/// </summary>
		public string Key { get; private set; }

		/// <summary>
		/// The address to request
		/// </summary>
		public string Address { get; private set; }

		/// <summary>
		/// Language of the request
		/// </summary>
		public string Language { get; set; }

		/// <summary>
		/// ISO code of the region
		/// </summary>
		public string Region { get; set; }

		/// <summary>
		/// Creates an instance for geocoding request
		/// </summary>
		/// <param name="key">The Google Maps API key to be used</param>
		public GeocodingRequest(string key) {
			Key = key;
		}

		/// <summary>
		/// Gets the request URL for the set parameters
		/// </summary>
		public string GetURL() {
			if (Key.IsNullOrEmpty()) 
				throw new Exception("No key provided");

			if (Address.IsNullOrEmpty())
				throw new Exception("No address provided");

			var builder = new StringBuilder(k_BaseURL);
			builder.Append("key=").Append(Key);
			builder.Append("&address=").Append(Address);

			if (!Language.IsNullOrEmpty())
				builder.Append("&language=").Append(Language);

			if (!Region.IsNullOrEmpty())
				builder.Append("&region=").Append(Region);

			return builder.ToString();
		}

		/// <summary>
		/// Sends the request for a given address and returns a promise
		/// </summary>
		/// <param name="address">THe address that needs to be goecoded</param>
		/// <returns>The promise for the response</returns>
		public IPromise<GeocodingResponse> Send(string address) {
			var promise = new Promise<GeocodingResponse>();
			Send(
				address,
				response => promise.Resolve(response),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		/// <summary>
		/// Sends the request for a given address
		/// </summary>
		/// <param name="address">The address that needs to be geocoded</param>
		/// <param name="onSuccess">Callback on successful request</param>
		/// <param name="onFailure">Callback on unsuccessul request</param>
		public void Send(string address, Action<GeocodingResponse> onSuccess, Action<Exception> onFailure) {
			Address = address;
			var client = new RestClient();
			var request = new RestRequest(GetURL(), Method.GET);

			client.ExecuteAsync(request, (response, handle) => {
				if (response.IsSuccess()) {
					Dispatcher.Enqueue(() => {
						var model = JsonUtility.FromJson<GeocodingResponse>(response.Content);
					
						if (model != null)
							onSuccess.TryInvoke(model);
						else {
							var exception = new Exception("Could not deserialize", response.GetException());
							onFailure.TryInvoke(exception);
						}
					});
				}
				else {
					Dispatcher.Enqueue(() => {
						var exception = new Exception("Unsuccessful response for Geocoding", response.GetException());
						onFailure.TryInvoke(exception);
					});
				}
			});
		}
	}
}