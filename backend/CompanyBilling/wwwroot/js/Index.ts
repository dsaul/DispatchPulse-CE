import { DateTime } from 'luxon';

export class Index {
	public static Start(): void {
		console.debug('Hello world! Index  ', DateTime);
	}
}

(window as any).DEBUG_Index = Index;

export default {};