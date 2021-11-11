import { DateTime, DurationObjectUnits } from 'luxon';

export interface IGetPayPeriodsNextToDate {
	currentPeriodStart: DateTime;
	currentPeriodEnd: DateTime;
	nextPeriodStart: DateTime;
	nextPeriodEnd: DateTime;
	lastPeriodStart: DateTime;
	lastPeriodEnd: DateTime;
	
	historicPeriods: IHistoricPeriod[];
}

export interface IHistoricPeriod {
	start: DateTime;
	end: DateTime;
}

export default (date?: DateTime, periodLength?: number, refStart?: DateTime): IGetPayPeriodsNextToDate | null => {
	// https://en.wikipedia.org/wiki/ISO_8601#Durations
	
	if (!date) {
		date = DateTime.local();
	}
	if (!periodLength) {
		periodLength = 14;
	}
	if (!refStart) {
		refStart = DateTime.local(2015, 5, 30);
	}
	
	
	
	date = date.set({
		hour: 0,
		minute: 0,
		second: 0,
	});
	
	//console.log("date");
	//console.log(date);
	
	const diff = date.diff(refStart, 'days');
	const diffObj = diff.toObject() as DurationObjectUnits;
	const days = diffObj.days as number;
	const daysIntoCurrentPeriod = Math.round(days % periodLength);
	
	//console.log("daysIntoCurrentPeriod " + daysIntoCurrentPeriod);
	
	const periodStart = date.minus({ days: daysIntoCurrentPeriod });
	const periodEnd = periodStart.plus({ days: periodLength - 1, hours: 23, minutes: 59, seconds: 59 });
	
	const o: IGetPayPeriodsNextToDate = {
		currentPeriodStart: periodStart,
		currentPeriodEnd: periodEnd,
		nextPeriodStart: periodStart.plus({ days: periodLength }),
		nextPeriodEnd: periodEnd.plus({ days: periodLength }),
		lastPeriodStart: periodStart.minus({ days: periodLength }),
		lastPeriodEnd: periodEnd.minus({ days: periodLength }),
		historicPeriods: [],
		
	};
	
	for (let i = 0; i < 26; i++) {
		const p: IHistoricPeriod = {
			start: o.currentPeriodStart.minus({ days: periodLength * i }),
			end: o.currentPeriodEnd.minus({ days: periodLength * i }),
		};
		o.historicPeriods.push(p);
	}
	
	//console.log(o);
	
	return o;
};
