using System;
using System.Threading.Tasks;
using SharedCode.Linode;

namespace LinodeAPITest
{
	class Program
	{
		static async Task Main()
		{
			var info = await StorageUtils.GetBucketInfo("", "tts-cache");
		}
	}
}
