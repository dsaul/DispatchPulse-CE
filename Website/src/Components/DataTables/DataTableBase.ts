import { Component, Vue, Prop } from 'vue-property-decorator';
import Dialogues from '@/Utility/Dialogues';
import ISO8601ToLocalDatetime from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDatetime';
import ISO8601ToLocalDateOnly from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDateOnly';
import ISO8601LocalDateOnlyTimeSpan from '@/Utility/Formatters/ISO8601/ISO8601LocalDateOnlyTimeSpan';
import ISO8601Difference from '@/Utility/ISO8601Difference';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import { Project } from '@/Data/CRM/Project/Project';
import { Product } from '@/Data/CRM/Product/Product';
import { Agent } from '@/Data/CRM/Agent/Agent';
import { LabourType } from '@/Data/CRM/LabourType/LabourType';
import { LabourSubtypeException } from '@/Data/CRM/LabourSubtypeException/LabourSubtypeException';
import { LabourSubtypeHoliday} from '@/Data/CRM/LabourSubtypeHoliday/LabourSubtypeHoliday';
import { LabourSubtypeNonBillable } from '@/Data/CRM/LabourSubtypeNonBillable/LabourSubtypeNonBillable';


@Component({
	components: {
		
	},
})
export default class DataTableBase extends Vue {
	
	@Prop({ default: false }) public readonly disabled!: boolean;
	
	// Formatting methods
	protected DialoguesOpen = Dialogues.Open;
	protected ISO8601ToLocalDatetime = ISO8601ToLocalDatetime;
	protected ISO8601ToLocalDateOnly = ISO8601ToLocalDateOnly;
	protected ISO8601LocalDateOnlyTimeSpan = ISO8601LocalDateOnlyTimeSpan;
	protected ISO8601Difference = ISO8601Difference;
	protected AssignmentWorkDescriptionForId = Assignment.WorkDescriptionForId;
	protected ProjectNameForId = Project.NameForId;
	protected ProjectCombinedDescriptionForId = Project.CombinedDescriptionForId;
	protected ProjectAddressForId = Project.AddressForId;
	protected ProductNameForId = Product.NameForId;
	protected AgentNameForId = Agent.NameForId;
	protected LabourTypeIconForId = LabourType.IconForId;
	protected LabourTypeNameForId = LabourType.NameForId;
	protected LabourTypeIsBillableForId = LabourType.IsBillableForId;
	protected LabourTypeIsHolidayForId = LabourType.IsHolidayForId;
	protected LabourTypeIsNonBillableForId = LabourType.IsNonBillableForId;
	protected LabourTypeIsExceptionForId = LabourType.IsExceptionForId;
	protected LabourTypeIsPayOutBankedForId = LabourType.IsPayOutBankedForId;
	protected LabourSubtypeExceptionNameForId = LabourSubtypeException.NameForId;
	protected LabourSubtypeExceptionIconForId = LabourSubtypeException.IconForId;
	protected LabourSubtypeHolidayNameForId = LabourSubtypeHoliday.NameForId;
	protected LabourSubtypeHolidayIconForId = LabourSubtypeHoliday.IconForId;
	protected LabourSubtypeNonBillableNameForId = LabourSubtypeNonBillable.NameForId;
	protected LabourSubtypeNonBillableIconForId = LabourSubtypeNonBillable.IconForId;
}
