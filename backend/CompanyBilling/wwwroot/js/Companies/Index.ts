import { DateTime } from 'luxon';

export class Index {
	public static Start(): void {
		console.debug('Hello world! Company Index  ', DateTime);
	}
}

(window as any).DEBUG_Company_Index = Index;

export default {};