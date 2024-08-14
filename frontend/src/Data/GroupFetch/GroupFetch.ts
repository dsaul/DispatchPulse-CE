import { RPCRequestGroupGeneral } from "@/Data/GroupFetch/RPCRequestGroupGeneral";
import { RPCRequestGroupViewDashboard } from "@/Data/GroupFetch/RPCRequestGroupViewDashboard";
import { RPCMethod } from "@/RPC/RPCMethod";

export class GroupFetch {
	// RPC Methods
	public static RequestGroupGeneral = RPCMethod.Register<
		RPCRequestGroupGeneral
	>(new RPCRequestGroupGeneral());
	public static RequestGroupViewDashboard = RPCMethod.Register<
		RPCRequestGroupViewDashboard
	>(new RPCRequestGroupViewDashboard());
}
