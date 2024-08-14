import GenerateID from "@/Utility/GenerateID";
import { DateTime } from "luxon";
import _ from "lodash";
import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";
import store from "@/plugins/store/store";
import CaseInsensitivePropertyGet from "@/Utility/CaseInsensitivePropertyGet";
import { guid } from "@/Utility/GlobalTypes";
import ITracker from "@/Utility/ITracker";
import { BillingSessions } from "@/Data/Billing/BillingSessions/BillingSessions";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCRequestProducts } from "@/Data/CRM/Product/RPCRequestProducts";
import { RPCDeleteProducts } from "@/Data/CRM/Product/RPCDeleteProducts";
import { RPCPushProducts } from "@/Data/CRM/Product/RPCPushProducts";
import { RPCMethod } from "@/RPC/RPCMethod";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IProduct extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		name: string | null;
		quantityUnit: string | null;
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
	};
}

export class Product {
	// RPC Methods
	public static RequestProducts = RPCMethod.Register<RPCRequestProducts>(
		new RPCRequestProducts()
	);
	public static DeleteProducts = RPCMethod.Register<RPCDeleteProducts>(
		new RPCDeleteProducts()
	);
	public static PushProducts = RPCMethod.Register<RPCPushProducts>(
		new RPCPushProducts()
	);

	public static _RefreshTracker: { [id: string]: ITracker } = {};

	public static FetchForId(id: guid): IRoundTripRequest {
		const ret: IRoundTripRequest = {
			roundTripRequestId: GenerateID(),
			outboundRequestPromise: null,
			completeRequestPromise: null,
			_completeRequestPromiseResolve: null,
			_completeRequestPromiseReject: null
		};

		// If we have no id, reject.
		if (!id || IsNullOrEmpty(id)) {
			ret.outboundRequestPromise = Promise.reject();
			ret.completeRequestPromise = Promise.reject();
			return ret;
		}

		// Remove all trackers that are complete and older than 5 seconds.
		const keys = Object.keys(this._RefreshTracker);
		for (const key of keys) {
			const tracker: ITracker = this._RefreshTracker[key];
			if (!tracker.isComplete) {
				continue;
			}

			if (
				DateTime.utc() > tracker.lastRequestTimeUtc.plus({ seconds: 5 })
			) {
				delete this._RefreshTracker[key];
			}
		}

		// Check and see if we already have a request pending.
		const existing = this._RefreshTracker[id];
		if (existing) {
			return existing.rtr;
		}

		const product = Product.ForId(id);
		if (product) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(product);
			return ret;
		}

		// We'll need to request this.
		const rtrNew = Product.RequestProducts.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			limitToIds: [id]
		});

		ret.outboundRequestPromise = rtrNew.outboundRequestPromise;

		ret.completeRequestPromise = new Promise((resolve, reject) => {
			ret._completeRequestPromiseResolve = resolve;
			ret._completeRequestPromiseReject = reject;
		});

		// Handlers once we get a response.
		if (rtrNew.completeRequestPromise) {
			rtrNew.completeRequestPromise.then(() => {
				if (ret._completeRequestPromiseResolve) {
					ret._completeRequestPromiseResolve(Product.ForId(id));
				}
			});

			rtrNew.completeRequestPromise.catch((e: Error) => {
				if (ret._completeRequestPromiseReject) {
					ret._completeRequestPromiseReject(e);
				}
			});

			rtrNew.completeRequestPromise.finally(() => {
				this._RefreshTracker[id].isComplete = true;
			});
		}

		this._RefreshTracker[id] = {
			lastRequestTimeUtc: DateTime.utc(),
			isComplete: false,
			rtr: rtrNew
		};

		return ret;
	}

	public static GetMerged(mergeValues: Record<string, any>): IProduct {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IProduct {
		const id = GenerateID();
		const ret: IProduct = {
			id,
			json: {
				id,
				name: null,
				quantityUnit: null,
				lastModifiedBillingId: null,
				lastModifiedISO8601: DateTime.utc().toISO()
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static ForId(id: string | null): IProduct | null {
		if (!id) {
			return null;
		}

		const products = store.state.Database.products as Record<
			string,
			IProduct
		>;
		if (!products || Object.keys(products).length === 0) {
			return null;
		}

		let product = products[id];
		if (!product || !product.json) {
			product = CaseInsensitivePropertyGet(products, id);
		}
		if (!product || !product.json) {
			return null;
		}

		return product;
	}

	public static UpdateIds(payload: Record<string, IProduct>): void {
		store.commit("UpdateProducts", payload);
	}

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteProducts", ids);
	}

	public static NameForId(id: string | null): string | null {
		if (!id) {
			return null;
		}

		const product = Product.ForId(id);
		if (!product || !product.json || IsNullOrEmpty(product.json.name)) {
			return null;
		}

		return product.json.name;
	}

	public static ValidateObject(o: IProduct): IProduct {
		return o;
	}

	public static PermProductsCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.products.request-any") !== -1 ||
			perms.indexOf("crm.products.request-company") !== -1;
		//console.log(JSON.stringify(perms), ret);
		return ret;
	}

	public static PermProductsCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.products.push-any") !== -1 ||
			perms.indexOf("crm.products.push-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermProductsCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.products.delete-any") !== -1 ||
			perms.indexOf("crm.products.delete-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMExportProductsCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.export.product-definitions-csv") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

(window as any).DEBUG_Product = Product;

export default {};
