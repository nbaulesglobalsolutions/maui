using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Windows.Networking.Connectivity;
using static Microsoft.Maui.Essentials.Connectivity.ConnectivityNativeHelper;

namespace Microsoft.Maui.Networking
{
	partial class ConnectivityImplementation : IConnectivity
	{
		void StartListeners() =>
			NetworkChange.NetworkAvailabilityChanged += NetworkStatusChanged;

		void StopListeners() =>
			NetworkChange.NetworkAvailabilityChanged -= NetworkStatusChanged;

		void NetworkStatusChanged(object sender, EventArgs e) =>
			OnConnectivityChanged();

		public NetworkAccess NetworkAccess
		{
			get
			{
				if (OperatingSystem.IsWindowsVersionAtLeast(11))
				{
					var profile = NetworkInformation.GetInternetConnectionProfile();
					if (profile == null)
						return NetworkAccess.Unknown;

					var level = profile.GetNetworkConnectivityLevel();
					return level switch
					{
						NetworkConnectivityLevel.LocalAccess => NetworkAccess.Local,
						NetworkConnectivityLevel.InternetAccess => NetworkAccess.Internet,
						NetworkConnectivityLevel.ConstrainedInternetAccess => NetworkAccess.ConstrainedInternet,
						_ => NetworkAccess.None,
					};
				}
				else
				{
					// Windows 10 workaround for 
					var networkList = GetNetworkListManager();
					var enumNetworks = networkList.GetNetworks(NLM_ENUM_NETWORK.NLM_ENUM_NETWORK_CONNECTED);
					var connectivity = NLM_CONNECTIVITY.NLM_CONNECTIVITY_DISCONNECTED;

					foreach (INetwork networkInterface in enumNetworks)
					{
						if (networkInterface.IsConnected())
						{
							connectivity = networkInterface.GetConnectivity();
							break;
						}
					}

					if ((connectivity & (NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV4_INTERNET | NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV6_INTERNET)) != 0)
					{
						return NetworkAccess.Internet;
					}
					else if ((connectivity & (NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV4_LOCALNETWORK | NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV6_LOCALNETWORK)) != 0)
					{
						return NetworkAccess.Local;
					}
					else if ((connectivity & (NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV4_NOTRAFFIC | NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV6_NOTRAFFIC)) != 0)
					{
						return NetworkAccess.Local;
					}
					else
					{
						return NetworkAccess.None;
					}
				}
			}
		}

		public IEnumerable<ConnectionProfile> ConnectionProfiles
		{
			get
			{
				var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (var nic in networkInterfaces)
				{
					if (nic.OperationalStatus is not OperationalStatus.Up ||
						nic.NetworkInterfaceType is NetworkInterfaceType.Loopback ||
						nic.NetworkInterfaceType is NetworkInterfaceType.Tunnel)
					{
						continue;
					}

					var interfaceType = ConnectionProfile.Unknown;
					switch (nic.NetworkInterfaceType)
					{
						case NetworkInterfaceType.Ethernet:
							interfaceType = ConnectionProfile.Ethernet;
							break;
						case NetworkInterfaceType.Wireless80211:
							interfaceType = ConnectionProfile.WiFi;
							break;
						case NetworkInterfaceType.Wwanpp:
						case NetworkInterfaceType.Wwanpp2:
							interfaceType = ConnectionProfile.Cellular;
							break;
					}

					yield return interfaceType;
				}
			}
		}
	}
}
