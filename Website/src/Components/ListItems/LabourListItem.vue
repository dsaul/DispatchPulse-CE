<template>
	<v-list-item>
		<v-list-item-avatar>
			<v-icon v-if="value.json.isActive">timer</v-icon>
			<v-icon v-else>timelapse</v-icon>
		</v-list-item-avatar>
		
		<v-list-item-content
			@click="ClickEntry">
			
			
			<v-list-item-title style="white-space: normal;" v-if="Type && Type.json.isBillable">
				
				
				<span v-if="focusIsProject">
					
					
					<DataLoaderProject v-if="ProjectId" :projectId="ProjectId">
						
						<div v-if="!Project && ProjectId">
							Project {{ProjectId}} (Not Found)
						</div>
						
						<span v-if="ProjectAddresses && ProjectAddresses.length > 0">
							<span v-for="(addr, index) of ProjectAddresses" :key="addr.id">
								<span v-if="(index == ProjectAddresses.length - 1) && ProjectAddresses.length > 1">and </span>
								{{addr.value}}<span v-if="(index != ProjectAddresses.length - 1) && ProjectAddresses.length > 1">, </span>
							</span>
							<span v-if="ProjectName">. </span>
						</span>
						<span v-if="ProjectName">
							{{ProjectName}}
						</span>
					
					</DataLoaderProject>
					
					
					
				</span>
				<span v-else-if="focusIsAgent">
					{{AgentName}}
				</span>
				<span v-else>
					Unexpected focus.
				</span>
			</v-list-item-title>
			
			
			
			<v-list-item-title v-if="Type && HolidayType && Type.json.isHoliday">
				{{HolidayType.json.name}}
			</v-list-item-title>
			
			<v-list-item-title v-if="Type && NonBillableType && Type.json.isNonBillable">
				<span v-if="focusIsProject">
					{{NonBillableType.json.name}} for 
					<span v-if="ProjectAddresses && ProjectAddresses.length > 0">
						<span v-for="(addr, index) of ProjectAddresses" :key="addr.id">
							<span v-if="(index == ProjectAddresses.length - 1) && ProjectAddresses.length > 1">and </span>
							{{addr.value}}<span v-if="(index != ProjectAddresses.length - 1) && ProjectAddresses.length > 1">, </span>
						</span>
						<span v-if="ProjectName">. </span>
					</span>
					<span v-if="ProjectName">
						{{ProjectName}}
					</span>
				</span>
				<span v-else-if="focusIsAgent">
					{{AgentName}}: {{NonBillableType.json.name}}
				</span>
			</v-list-item-title>
			
			<v-list-item-title v-if="Type && ExceptionType && Type.json.isException">
				{{ExceptionType.json.name}}
			</v-list-item-title>
			
			<v-list-item-title v-if="Type && Type.json.isPayOutBanked">
				{{Type.json.name}}
			</v-list-item-title>
			
			<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/"
				v-if="Type && Type.json.isBillable"
			>
				
				
				
				
				
				
				
				<v-tooltip
					v-if="value.json.isActive"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Active
						</v-chip>
					</template>
					<span>Whether this labour entry is currently progressing.</span>
				</v-tooltip>
				
				<span v-if="value.json.timeMode == 'date-and-hours' && value.json.hours">
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-avatar left>
									<v-icon small>timelapse</v-icon>
								</v-avatar>
								{{`${Math.floor(+value.json.hours)}`.padStart(2, '0')}}h
								{{(60 * ((+value.json.hours) % 1)).toFixed(0).padStart(2, '0')}}m
								00s
							</v-chip>
						</template>
						<span>Duration</span>
					</v-tooltip>
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-avatar left>
									<v-icon small>fa-hourglass-start</v-icon>
								</v-avatar>
								{{StartLocalJustDate}}
							</v-chip>
						</template>
						<span>Date</span>
					</v-tooltip>
				</span>
				<span v-else-if="value.json.timeMode == 'start-stop-timestamp'">
					<span v-if="false == value.json.isActive && Difference">
						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>timelapse</v-icon>
									</v-avatar>
									{{(''+Difference.hours).padStart(2, '0')}}h <!--
								-->{{(''+Difference.minutes).padStart(2, '0')}}m <!--
								-->{{(''+Difference.seconds.toFixed(0)).padStart(2, '0')}}s
								</v-chip>
							</template>
							<span>Duration</span>
						</v-tooltip>

						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>fa-hourglass-start</v-icon>
									</v-avatar>
									{{StartLocale}}
								</v-chip>
							</template>
							<span>Start Time</span>
						</v-tooltip>
						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>fa-hourglass-end</v-icon>
									</v-avatar>
									{{EndLocale}}
								</v-chip>
							</template>
							<span>End Time</span>
						</v-tooltip>
					</span>
					<v-tooltip
						v-else-if="true == value.json.isActive"
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								> <!-- active -->
								<!--Started {{StartLocale}}.-->
								<v-avatar left>
									<v-icon small>timer</v-icon>
								</v-avatar>
								<span v-if="currentDifferenceToNow">
									
									{{(''+currentDifferenceToNow.hours).padStart(2, '0')}}h <!--
									-->{{(''+currentDifferenceToNow.minutes).padStart(2, '0')}}m <!--
									-->{{(''+currentDifferenceToNow.seconds.toFixed(0)).padStart(2, '0')}}s
									
									
								</span>
							</v-chip>
						</template>
						<span>Start Time</span>
					</v-tooltip>

				</span>
				<v-tooltip
					v-else
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							<v-avatar left>
								<v-icon small>timelapse</v-icon>
							</v-avatar>
							No time specified.
						</v-chip>
					</template>
					<span>There is no time for this labour entry.</span>
				</v-tooltip>

				<v-tooltip
					v-if="Type && Type.json.name"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							{{Type.json.name}}
						</v-chip>
					</template>
					<span>The labour type for this entry.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'travel'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Travel
						</v-chip>
					</template>
					<span>This entry is for travelling.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'on-site'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							On-site
						</v-chip>
					</template>
					<span>This entry is for on-site work.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'remote'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Remote
						</v-chip>
					</template>
					<span>This entry is for remote work.</span>
				</v-tooltip>
				<v-tooltip
					v-if="LabourIsExtraForId(value.id)"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Extra
						</v-chip>
					</template>
					<span>This work is not included in a contract.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.isBilled"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Billed
						</v-chip>
					</template>
					<span>This has been billed the client.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.isPaidOut"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Paid Out
						</v-chip>
					</template>
					<span>This has been paid out to the agent.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.notes"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Has Notes
						</v-chip>
					</template>
					<span>This entry has additional notes.</span>
				</v-tooltip>
				
				<v-tooltip
					v-if="rootProject && rootProject.id !== value.json.projectId"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							color="primary"
							@click.stop.prevent.once=""
							:to="`/section/projects/${value.json.projectId}?tab=General`"
							>
							From a Child Project
						</v-chip>
					</template>
					<span>{{ProjectNameForId(value.id) || 'No Name'}}</span>
				</v-tooltip>

			</v-list-item-subtitle>
			
			<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/"
				v-if="Type && Type.json.isHoliday"
			>
				<v-tooltip
					v-if="value.json.isActive"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Active
						</v-chip>
					</template>
					<span>Whether this labour entry is currently progressing.</span>
				</v-tooltip>
				
				<span v-if="value.json.timeMode == 'date-and-hours' && value.json.hours">
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-avatar left>
									<v-icon small>timelapse</v-icon>
								</v-avatar>
								{{`${Math.floor(+value.json.hours)}`.padStart(2, '0')}}h
								{{(60 * ((+value.json.hours) % 1)).toFixed(0).padStart(2, '0')}}m
								00s
							</v-chip>
						</template>
						<span>Duration</span>
					</v-tooltip>
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-avatar left>
									<v-icon small>fa-hourglass-start</v-icon>
								</v-avatar>
								{{StartLocalJustDate}}
							</v-chip>
						</template>
						<span>Date</span>
					</v-tooltip>
				</span>
				<span v-else-if="value.json.timeMode == 'start-stop-timestamp'">
					<span v-if="false == value.json.isActive && Difference">
						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>timelapse</v-icon>
									</v-avatar>
									{{(''+Difference.hours).padStart(2, '0')}}h <!--
								-->{{(''+Difference.minutes).padStart(2, '0')}}m <!--
								-->{{(''+Difference.seconds.toFixed(0)).padStart(2, '0')}}s
								</v-chip>
							</template>
							<span>Duration</span>
						</v-tooltip>

						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>fa-hourglass-start</v-icon>
									</v-avatar>
									{{StartLocale}}
								</v-chip>
							</template>
							<span>Start Time</span>
						</v-tooltip>
						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>fa-hourglass-end</v-icon>
									</v-avatar>
									{{EndLocale}}
								</v-chip>
							</template>
							<span>End Time</span>
						</v-tooltip>
					</span>
					<v-tooltip
						v-else-if="true == value.json.isActive"
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								> <!-- active -->
								<!--Started {{StartLocale}}.-->
								<v-avatar left>
									<v-icon small>timer</v-icon>
								</v-avatar>
								<span v-if="currentDifferenceToNow">
									
									{{(''+currentDifferenceToNow.hours).padStart(2, '0')}}h <!--
									-->{{(''+currentDifferenceToNow.minutes).padStart(2, '0')}}m <!--
									-->{{(''+currentDifferenceToNow.seconds.toFixed(0)).padStart(2, '0')}}s
									
									
								</span>
							</v-chip>
						</template>
						<span>Start Time</span>
					</v-tooltip>

				</span>
				<v-tooltip
					v-else
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							<v-avatar left>
								<v-icon small>timelapse</v-icon>
							</v-avatar>
							No time specified.
						</v-chip>
					</template>
					<span>There is no time for this labour entry.</span>
				</v-tooltip>
				
				
				<v-tooltip
					v-if="Type && Type.json.name"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							{{Type.json.name}}
						</v-chip>
					</template>
					<span>The labour type for this entry.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'travel'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Travel
						</v-chip>
					</template>
					<span>This entry is for travelling.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'on-site'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							On-site
						</v-chip>
					</template>
					<span>This entry is for on-site work.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'remote'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Remote
						</v-chip>
					</template>
					<span>This entry is for remote work.</span>
				</v-tooltip>
				<v-tooltip
					v-if="LabourIsExtraForId(value.id)"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Extra
						</v-chip>
					</template>
					<span>This work is not included in a contract.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.isBilled"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Billed
						</v-chip>
					</template>
					<span>This has been billed the client.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.isPaidOut"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Paid Out
						</v-chip>
					</template>
					<span>This has been paid out to the agent.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.notes"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Has Notes
						</v-chip>
					</template>
					<span>This entry has additional notes.</span>
				</v-tooltip>
			</v-list-item-subtitle>
			
			<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/"
				v-if="Type && Type.json.isNonBillable"
			>
				<v-tooltip
					v-if="value.json.isActive"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Active
						</v-chip>
					</template>
					<span>Whether this labour entry is currently progressing.</span>
				</v-tooltip>
				
				<span v-if="value.json.timeMode == 'date-and-hours' && value.json.hours">
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-avatar left>
									<v-icon small>timelapse</v-icon>
								</v-avatar>
								{{`${Math.floor(+value.json.hours)}`.padStart(2, '0')}}h
								{{(60 * ((+value.json.hours) % 1)).toFixed(0).padStart(2, '0')}}m
								00s
							</v-chip>
						</template>
						<span>Duration</span>
					</v-tooltip>
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-avatar left>
									<v-icon small>fa-hourglass-start</v-icon>
								</v-avatar>
								{{StartLocalJustDate}}
							</v-chip>
						</template>
						<span>Date</span>
					</v-tooltip>
				</span>
				<span v-else-if="value.json.timeMode == 'start-stop-timestamp'">
					<span v-if="false == value.json.isActive && Difference">
						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>timelapse</v-icon>
									</v-avatar>
									{{(''+Difference.hours).padStart(2, '0')}}h <!--
								-->{{(''+Difference.minutes).padStart(2, '0')}}m <!--
								-->{{(''+Difference.seconds.toFixed(0)).padStart(2, '0')}}s
								</v-chip>
							</template>
							<span>Duration</span>
						</v-tooltip>

						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>fa-hourglass-start</v-icon>
									</v-avatar>
									{{StartLocale}}
								</v-chip>
							</template>
							<span>Start Time</span>
						</v-tooltip>
						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>fa-hourglass-end</v-icon>
									</v-avatar>
									{{EndLocale}}
								</v-chip>
							</template>
							<span>End Time</span>
						</v-tooltip>
					</span>
					<v-tooltip
						v-else-if="true == value.json.isActive"
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								> <!-- active -->
								<!--Started {{StartLocale}}.-->
								<v-avatar left>
									<v-icon small>timer</v-icon>
								</v-avatar>
								<span v-if="currentDifferenceToNow">
									
									{{(''+currentDifferenceToNow.hours).padStart(2, '0')}}h <!--
									-->{{(''+currentDifferenceToNow.minutes).padStart(2, '0')}}m <!--
									-->{{(''+currentDifferenceToNow.seconds.toFixed(0)).padStart(2, '0')}}s
									
									
								</span>
							</v-chip>
						</template>
						<span>Start Time</span>
					</v-tooltip>

				</span>
				<v-tooltip
					v-else
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							<v-avatar left>
								<v-icon small>timelapse</v-icon>
							</v-avatar>
							No time specified.
						</v-chip>
					</template>
					<span>There is no time for this labour entry.</span>
				</v-tooltip>

				<v-tooltip
					v-if="Type && Type.json.name"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							{{Type.json.name}}
						</v-chip>
					</template>
					<span>The labour type for this entry.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'travel'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Travel
						</v-chip>
					</template>
					<span>This entry is for travelling.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'on-site'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							On-site
						</v-chip>
					</template>
					<span>This entry is for on-site work.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'remote'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Remote
						</v-chip>
					</template>
					<span>This entry is for remote work.</span>
				</v-tooltip>
				<v-tooltip
					v-if="LabourIsExtraForId(value.id)"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Extra
						</v-chip>
					</template>
					<span>This work is not included in a contract.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.isBilled"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Billed
						</v-chip>
					</template>
					<span>This has been billed the client.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.isPaidOut"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Paid Out
						</v-chip>
					</template>
					<span>This has been paid out to the agent.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.notes"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Has Notes
						</v-chip>
					</template>
					<span>This entry has additional notes.</span>
				</v-tooltip>
				
			</v-list-item-subtitle>
			
			<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/"
				v-if="Type && Type.json.isException"
			>
				<v-tooltip
					v-if="value.json.isActive"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Active
						</v-chip>
					</template>
					<span>Whether this labour entry is currently progressing.</span>
				</v-tooltip>
				
				<span v-if="value.json.timeMode == 'date-and-hours' && value.json.hours">
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-avatar left>
									<v-icon small>timelapse</v-icon>
								</v-avatar>
								{{`${Math.floor(+value.json.hours)}`.padStart(2, '0')}}h
								{{(60 * ((+value.json.hours) % 1)).toFixed(0).padStart(2, '0')}}m
								00s
							</v-chip>
						</template>
						<span>Duration</span>
					</v-tooltip>
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-avatar left>
									<v-icon small>fa-hourglass-start</v-icon>
								</v-avatar>
								{{StartLocalJustDate}}
							</v-chip>
						</template>
						<span>Date</span>
					</v-tooltip>
				</span>
				<span v-else-if="value.json.timeMode == 'start-stop-timestamp'">
					<span v-if="false == value.json.isActive && Difference">
						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>timelapse</v-icon>
									</v-avatar>
									{{(''+Difference.hours).padStart(2, '0')}}h <!--
								-->{{(''+Difference.minutes).padStart(2, '0')}}m <!--
								-->{{(''+Difference.seconds.toFixed(0)).padStart(2, '0')}}s
								</v-chip>
							</template>
							<span>Duration</span>
						</v-tooltip>

						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>fa-hourglass-start</v-icon>
									</v-avatar>
									{{StartLocale}}
								</v-chip>
							</template>
							<span>Start Time</span>
						</v-tooltip>
						<v-tooltip
							top
							>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip 
									label
									outlined
									small
									style="margin-right: 5px;"
									v-on="on"
									>
									<v-avatar left>
										<v-icon small>fa-hourglass-end</v-icon>
									</v-avatar>
									{{EndLocale}}
								</v-chip>
							</template>
							<span>End Time</span>
						</v-tooltip>
					</span>
					<v-tooltip
						v-else-if="true == value.json.isActive"
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								> <!-- active -->
								<!--Started {{StartLocale}}.-->
								<v-avatar left>
									<v-icon small>timer</v-icon>
								</v-avatar>
								<span v-if="currentDifferenceToNow">
									
									{{(''+currentDifferenceToNow.hours).padStart(2, '0')}}h <!--
									-->{{(''+currentDifferenceToNow.minutes).padStart(2, '0')}}m <!--
									-->{{(''+currentDifferenceToNow.seconds.toFixed(0)).padStart(2, '0')}}s
									
									
								</span>
							</v-chip>
						</template>
						<span>Start Time</span>
					</v-tooltip>

				</span>
				<v-tooltip
					v-else
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							<v-avatar left>
								<v-icon small>timelapse</v-icon>
							</v-avatar>
							No time specified.
						</v-chip>
					</template>
					<span>There is no time for this labour entry.</span>
				</v-tooltip>

				<v-tooltip
					v-if="Type && Type.json.name"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							{{Type.json.name}}
						</v-chip>
					</template>
					<span>The labour type for this entry.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'travel'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Travel
						</v-chip>
					</template>
					<span>This entry is for travelling.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'on-site'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							On-site
						</v-chip>
					</template>
					<span>This entry is for on-site work.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.locationType == 'remote'"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Remote
						</v-chip>
					</template>
					<span>This entry is for remote work.</span>
				</v-tooltip>
				<v-tooltip
					v-if="LabourIsExtraForId(value.id)"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Extra
						</v-chip>
					</template>
					<span>This work is not included in a contract.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.isBilled"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Billed
						</v-chip>
					</template>
					<span>This has been billed the client.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.isPaidOut"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Paid Out
						</v-chip>
					</template>
					<span>This has been paid out to the agent.</span>
				</v-tooltip>
				<v-tooltip
					v-if="value.json.notes"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							>
							Has Notes
						</v-chip>
					</template>
					<span>This entry has additional notes.</span>
				</v-tooltip>
				
			</v-list-item-subtitle>
			
			<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/"
				v-if="Type && Type.json.isPayOutBanked"
			>
				<span v-if="value.json.bankedPayOutAmount">${{value.json.bankedPayOutAmount}}. </span>
				<span v-if="value.json.notes"> {{value.json.notes}} </span>
			</v-list-item-subtitle>
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
			
		</v-list-item-content>
		
		
		
		
		<v-list-item-action v-if="value.json.isActive">
			<v-btn
				icon
				color="primary"
				@click="DialoguesOpen({
					name: 'LabourDeleteTimerDialogue',
					state: {
						labourId: value.id
					}
					})"
				>
				<v-icon>close</v-icon>
			</v-btn>
		</v-list-item-action>
		<v-list-item-action v-if="value.json.isActive">
			<v-btn
				icon
				color="primary"
				@click="DialoguesOpen({
					name: 'ModifyLabourDialogue',
					state: GetEntryCloneConfirmed()
					})"
				>
				<v-icon>done</v-icon>
			</v-btn>
		</v-list-item-action>
		
		
		
		<v-list-item-action v-if="showMenuButton && !value.json.isActive">
			<v-menu bottom left>
				<template v-slot:activator="{ on }">
					<v-btn
						icon
						v-on="on"
						:disabled="disabled"
						>
						<v-icon>more_vert</v-icon>
					</v-btn>
				</template>

				<v-list dense>
					<v-list-item
						:disabled="isDialogue || disabled"
						@click="$emit('OpenEntry', value.id)"
						
						>
						<v-list-item-icon>
							<v-icon>open_in_new</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Open…</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="$emit('DeleteEntry', value.id)"
						:disabled="disabled || !PermProductsCanDelete()"
						>
						<v-list-item-icon>
							<v-icon>delete</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Delete…</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-divider></v-divider>
					<v-list-item
						@click="GoToProject()"
						:disabled="disabled"
						>
						<v-list-item-icon>
							<v-icon>assignment</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Go to Project…</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="GoToAgent()"
						:disabled="disabled"
						>
						<v-list-item-icon>
							<v-icon>directions_walk</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Go to Agent…</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</v-list>
			</v-menu>
		</v-list-item-action>
	</v-list-item>


</template>

<script lang="ts">
import Dialogues from '@/Utility/Dialogues';
import { Component, Vue, Prop } from 'vue-property-decorator';
import { DurationObjectUnits, DateTime } from 'luxon';
import ListItemBase from './ListItemBase';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import _ from 'lodash';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { ILabourSubtypeHoliday, LabourSubtypeHoliday } from '@/Data/CRM/LabourSubtypeHoliday/LabourSubtypeHoliday';
import { ILabourSubtypeException, LabourSubtypeException } from '@/Data/CRM/LabourSubtypeException/LabourSubtypeException';
import { ILabourSubtypeNonBillable, LabourSubtypeNonBillable } from '@/Data/CRM/LabourSubtypeNonBillable/LabourSubtypeNonBillable';
import { ILabourType, LabourType } from '@/Data/CRM/LabourType/LabourType';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import DataLoaderProject from '@/Components/DataLoaders/DataLoaderProject.vue';
import { Product } from '@/Data/CRM/Product/Product';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { IAddress } from '@/Data/Models/Address/Address';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		DataLoaderProject,
	},
})
export default class LabourListItem extends ListItemBase {
	
	@Prop({ default: null }) declare public readonly value: ILabour;
	@Prop({ default: null }) public readonly rootProject!: IProject;
	@Prop({ default: false }) public readonly focusIsProject!: boolean;
	@Prop({ default: false }) public readonly focusIsAgent!: boolean;
	
	protected ProjectNameForId = Project.NameForId;
	protected LabourIsExtraForId = Labour.IsExtraForId;
	protected DialoguesOpen = Dialogues.Open;
	protected PermProductsCanDelete = Product.PermProductsCanDelete;
	
	protected project: IProject | null = null;
	protected currentDifferenceToNowTimer: ReturnType<typeof setTimeout> | null = null;
	protected currentDifferenceToNow: DurationObjectUnits | null = null;
	
	protected loadingData = false;
	
	
	public LoadData(): void {
		
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				if (this.value == null) {
					return;
				}
				
				const promises: Array<Promise<any>> = [];
				
				if (null != this.value.json.projectId && !IsNullOrEmpty(this.value.json.projectId)) {
					
					const project = Project.ForId(this.value.json.projectId);
					if (null == project && Project.PermProjectsCanRequest()) {
						
						const rtr = Project.FetchForId(this.value.json.projectId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
						
					}
				}
				
				
				if (null != this.value.json.agentId && !IsNullOrEmpty(this.value.json.agentId)) {
					
					const agent = Agent.ForId(this.value.json.agentId);
					if (null == agent && Agent.PermAgentsCanRequest()) {
						
						const agentRTR = Agent.FetchForId(this.value.json.agentId);
						if (agentRTR.completeRequestPromise) {
							promises.push(agentRTR.completeRequestPromise);
						}
						
					}
				}
				
				if (null != this.value.json.assignmentId && !IsNullOrEmpty(this.value.json.assignmentId)) {
					
					const assignment = Assignment.ForId(this.value.json.assignmentId);
					if (null == assignment && Assignment.PermAssignmentCanRequest()) {
						
						const rtr = Assignment.FetchForId(this.value.json.assignmentId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
						
					}
				}
				
				if (null != this.value.json.typeId && !IsNullOrEmpty(this.value.json.typeId)) {
					
					const labourType = LabourType.ForId(this.value.json.typeId);
					if (null == labourType && LabourType.PermLabourTypesCanRequest()) {
						
						const rtr = LabourType.FetchForId(this.value.json.typeId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
						
					}
				}
				
				if (null != this.value.json.exceptionTypeId && !IsNullOrEmpty(this.value.json.exceptionTypeId)) {
					
					const exceptionType = LabourSubtypeException.ForId(this.value.json.exceptionTypeId);
					if (null == exceptionType && LabourSubtypeException.PermLabourSubtypeExceptionCanRequest()) {
						
						const rtr = LabourSubtypeException.FetchForId(this.value.json.exceptionTypeId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
						
					}
				}
				
				if (null != this.value.json.holidayTypeId && !IsNullOrEmpty(this.value.json.holidayTypeId)) {
					
					const holidayType = LabourSubtypeHoliday.ForId(this.value.json.holidayTypeId);
					if (null == holidayType && LabourSubtypeHoliday.PermLabourSubtypeHolidayCanRequest()) {
						
						const rtr = LabourSubtypeHoliday.FetchForId(this.value.json.holidayTypeId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
						
					}
				}
				
				if (null != this.value.json.nonBillableTypeId && !IsNullOrEmpty(this.value.json.nonBillableTypeId)) {
					
					const nonBillableType = LabourSubtypeNonBillable.ForId(this.value.json.nonBillableTypeId);
					if (null == nonBillableType && LabourSubtypeNonBillable.PermLabourSubtypeNonBillableCanRequest()) {
						
						const rtr = LabourSubtypeNonBillable.FetchForId(this.value.json.nonBillableTypeId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
						
					}
				}
				
				if (promises.length > 0) {
					
					this.loadingData = true;
					
					Promise.all(promises).finally(() => {
						this.loadingData = false;
					});
				}
				
				
			});
		});
		
		
		
		
		
		
		
		
	}
	
	
	
	
	
	
	
	public MountedAfter(): void {
		//console.debug('LLI value', this.value, this.Type);
		
		this.currentDifferenceToNowTimer = setInterval(() => {
			Vue.set(this, 'currentDifferenceToNow', this.GetDifferenceToNow());
		}, 1000);
	}
	
	public DestroyedAfter(): void {
		if (this.currentDifferenceToNowTimer) {
			clearInterval(this.currentDifferenceToNowTimer);
			this.currentDifferenceToNowTimer = null;
		}
		
		
	}
	
	protected get Type(): ILabourType | null {
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.typeId ||
			IsNullOrEmpty(this.value.json.typeId)
			) {
			return null;
		}
		
		return LabourType.ForId(this.value.json.typeId);
	}
	
	protected get NonBillableType(): ILabourSubtypeNonBillable | null {
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.nonBillableTypeId ||
			IsNullOrEmpty(this.value.json.nonBillableTypeId)
			) {
			return null;
		}
		
		return LabourSubtypeNonBillable.ForId(this.value.json.nonBillableTypeId);
	}
	
	protected get ExceptionType(): ILabourSubtypeException | null {
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.exceptionTypeId ||
			IsNullOrEmpty(this.value.json.exceptionTypeId)
			) {
			return null;
		}
		
		return LabourSubtypeException.ForId(this.value.json.exceptionTypeId);
	}
	
	protected get HolidayType(): ILabourSubtypeHoliday | null {
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.holidayTypeId ||
			IsNullOrEmpty(this.value.json.holidayTypeId)
			) {
			return null;
		}
		
		return LabourSubtypeHoliday.ForId(this.value.json.holidayTypeId);
	}
	
	
	protected get Agent(): IAgent | null {
		if (!this.value ||
			!this.value.json || 
			!this.value.json.agentId ||
			IsNullOrEmpty(this.value.json.agentId)
			) {
			return null;
		}
		
		const agent = Agent.ForId(this.value.json.agentId);
		if (!agent) {
			return null;
		}
		
		return agent;
	}
	
	protected get ProjectId(): guid | null {
		
		if (!this.value ||
			!this.value.json) {
			return null;
		}
		
		return this.value.json.projectId;
	}
	
	protected get Project(): IProject | null {
		const id = this.ProjectId;
		if (!id) {
			return null;
		}
		
		const project = Project.ForId(this.value.json.projectId);
		if (!project) {
			return null;
		}
		
		return project;
	}
	
	protected get ProjectName(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.projectId ||
			IsNullOrEmpty(this.value.json.projectId)) {
			return null;
		}
		
		return Project.NameForId(this.value.json.projectId);
	}
	
	protected get ProjectAddresses(): IAddress[] {
		
		const project = this.Project;
		if (!project) {
			return [];
		}
		
		return project.json.addresses;
	}
	
	protected get AgentName(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.agentId ||
			IsNullOrEmpty(this.value.json.agentId)) {
			return null;
		}
		
		const agent = Agent.ForId(this.value.json.agentId);
		if (!agent) {
			return null;
		}
		
		return agent.json.name;
	}
	
	protected get AgentTitle(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.agentId ||
			IsNullOrEmpty(this.value.json.agentId)) {
			return null;
		}
		
		const agent = Agent.ForId(this.value.json.agentId);
		if (!agent) {
			return null;
		}
		
		return agent.json.title;
	}
	
	protected get StartLocalJustDate(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.startISO8601 ||
			IsNullOrEmpty(this.value.json.startISO8601)) {
			return null;
		}
		
		const startISO = DateTime.fromISO(this.value.json.startISO8601);
		const startLocal = startISO.toLocal();
		
		return startLocal.toLocaleString(DateTime.DATE_SHORT);
		
	}
	
	protected get StartLocale(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.startISO8601 ||
			IsNullOrEmpty(this.value.json.startISO8601)) {
			return null;
		}
		
		const utc = DateTime.fromISO(this.value.json.startISO8601);
		const local = utc.toLocal();
		return local.toLocaleString(DateTime.DATETIME_SHORT);
	}
	
	protected get EndLocale(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.endISO8601 ||
			IsNullOrEmpty(this.value.json.endISO8601)) {
			return null;
		}
		
		const utc = DateTime.fromISO(this.value.json.endISO8601);
		const local = utc.toLocal();
		return local.toLocaleString(DateTime.DATETIME_SHORT);
	}
	
	protected get Difference(): DurationObjectUnits | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.startISO8601 ||
			IsNullOrEmpty(this.value.json.startISO8601) ||
			!this.value.json.endISO8601 ||
			IsNullOrEmpty(this.value.json.endISO8601)) {
			return null;
		}
		
		
		
		const startISO = DateTime.fromISO(this.value.json.startISO8601);
		const startLocal = startISO.toLocal();
		const endISO = DateTime.fromISO(this.value.json.endISO8601);
		const endLocal = endISO.toLocal();
		
		return endLocal.diff(startLocal, ['hours', 'minutes', 'seconds']).toObject();
	}
	
	protected GetDifferenceToNow(): DurationObjectUnits | null {
		
		//console.log('GetDifferenceToNow');
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.startISO8601 ||
			IsNullOrEmpty(this.value.json.startISO8601)
			) {
			return null;
		}
		
		
		
		const startISO = DateTime.fromISO(this.value.json.startISO8601);
		const startLocal = startISO.toLocal();
		const diff = DateTime.local().diff(startLocal, ['hours', 'minutes', 'seconds']).toObject();
		
		//console.log('diff', diff);
		
		return diff;
	}
	
	protected GetEntryCloneConfirmed(): ILabour {
		
		const clone = _.cloneDeep(this.value) as ILabour;
		clone.json.isActive = false;
		clone.json.endISO8601 = this.GetCurrentISO8601();
		return clone;
	}
	
	
	protected GetCurrentISO8601(): string {
		return DateTime.utc().toISO();
	}
	
	
	protected GoToProject(): void {
		//console.log('GoToProject()', this.value);
		
		const projectId = this.value.json.projectId;
		if (IsNullOrEmpty(projectId)) {
			console.warn('Unable to open project as project is null or empty');
			return;
		}
		
		this.$router.push(`/section/projects/${projectId}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		
		
	}
	
	protected GoToAgent(): void {
		
		//console.log('GoToAgent()', this.value);
		
		const agentId = this.value.json.agentId;
		if (IsNullOrEmpty(agentId)) {
			console.warn('Unable to open agent as agent is null or empty');
			return;
		}
		
		this.$router.push(`/section/agents/${agentId}?tab=Agenda`).catch(((e: Error) => { }));// eslint-disable-line
	}
	
	protected ClickEntry(): void {
		if (this.value && this.value.json && !IsNullOrEmpty(this.value.id)) {
			this.$emit('ClickEntry', this.value.id);
		}
	}
	
	
	
	
	
}

</script>