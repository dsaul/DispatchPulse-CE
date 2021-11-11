<template>
	<div>
	
		<v-app-bar
			v-if="showAppBar"
			color="#747389"
			dark
			fixed
			app
			clipped-right
			>
			<v-progress-linear
				v-if="isLoadingData"
				:indeterminate="true"
				absolute
				top
				color="white"
			></v-progress-linear>
			<v-app-bar-nav-icon @click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>
			
			<v-toolbar-title class="white--text">Project</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->
			
			<NotificationBellButton />
			<HelpMenuButton>
				<v-list-item
					@click="OnlineHelpFiles()"
					>
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Project Tutorial Pages</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</HelpMenuButton>
			<ReloadButton @reload="$emit('reload')" />

			<!--<CommitSessionGlobalButton />-->

			<v-menu bottom left offset-y>
				<template v-slot:activator="{ on }">
					<v-btn
					dark
					icon
					v-on="on"
					>
						<v-icon>more_vert</v-icon>
					</v-btn>
				</template>

				<v-list dense>
					<v-list-item
						@click="DoPrint()"
						:disabled="connectionStatus != 'Connected' || !PermCRMReportProjectsPDF()"
						>
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="CSVDownloadProject(value)"
						:disabled="!PermCRMExportProjectsCSV()"
						>
						<v-list-item-icon>
							<v-icon>import_export</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Export to CSV&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</v-list>
			</v-menu>
			
			
			<template v-slot:extension>
				<v-tabs
				v-model="tab"
				
				background-color="transparent"
				align-with-title
				show-arrows
				>
					<v-tabs-slider color="white"></v-tabs-slider>

					<v-tab 
						:disabled="!value"
						@click="$router.replace({query: { ...$route.query, tab: 'General'}}).catch(((e) => {}));"
						>
						General
					</v-tab>
					<v-tab
						:disabled="!value"
						@click="$router.replace({query: { ...$route.query, tab: 'Schedule'}}).catch(((e) => {}));"
						>
						Schedule
					</v-tab>
					<v-tab
						:disabled="!value || !PermProjectNotesCanRequest()"
						@click="$router.replace({query: { ...$route.query, tab: 'Notes'}}).catch(((e) => {}));"
						>
						Notes
					</v-tab>
					<v-tab
						:disabled="!value || !PermAssignmentCanRequest()"
						@click="$router.replace({query: { ...$route.query, tab: 'Assignments'}}).catch(((e) => {}));"
						>
						Assignments
					</v-tab>
					<v-tab 
						:disabled="!value || !PermMaterialsCanRequest()"
						@click="$router.replace({query: { ...$route.query, tab: 'Materials'}}).catch(((e) => {}));"
						>
						Materials
					</v-tab>
					<v-tab 
						:disabled="!value || !PermLabourCanRequest()"
						@click="$router.replace({query: { ...$route.query, tab: 'Labour'}}).catch(((e) => {}));"
						>
						Labour
					</v-tab>
					<v-tab
						:disabled="!value"
						@click="$router.replace({query: { ...$route.query, tab: 'Settings'}}).catch(((e) => {}));"
						>
						Settings
					</v-tab>
				</v-tabs>
			</template>
			
		</v-app-bar>
		
		<v-breadcrumbs
			v-if="breadcrumbs"
			:items="breadcrumbs"
			style="background: white; padding-bottom: 5px; padding-top: 15px;"
			>
			<template v-slot:divider>
				<v-icon>mdi-forward</v-icon>
			</template>
		</v-breadcrumbs>
		
		<v-alert
			v-if="connectionStatus != 'Connected'"
			type="error"
			elevation="2"
			style="margin-top: 10px; margin-left: 15px; margin-right: 15px;"
			>
			Disconnected from server.
		</v-alert>
		
		<div v-if="!value" style="margin-top: 20px;" class="fadeIn404">
			<v-container>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						<div class="title">Project Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The project no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the project while you were opening it.</li>
							<li>There is trouble connecting to the internet.</li>
							<li>Your mobile phone is in a place with a poor connection.</li>
							<li>Other reasons the app can't connect to Dispatch Pulse.</li>
						</ul>
					</v-col>
				</v-row>
			</v-container>
		</div>
		<div v-else>
		
			<v-tabs
				v-if="!showAppBar"
				v-model="tab"
				background-color="transparent"
				grow
				show-arrows
			>
				<v-tab>
					General
				</v-tab>
				<v-tab
					class="e2e-project-editor-in-page-tab-schedule"
					>
					Schedule
				</v-tab>
				<v-tab :disabled="isMakingNew || !PermProjectNotesCanRequest()">
					Notes
				</v-tab>
				<v-tab :disabled="isMakingNew || !PermAssignmentCanRequest()">
					Assignments
				</v-tab>
				<v-tab :disabled="isMakingNew || !PermMaterialsCanRequest()">
					Materials
				</v-tab>
				<v-tab :disabled="isMakingNew || !PermLabourCanRequest()">
					Labour
				</v-tab>
				<v-tab>
					Settings
				</v-tab>
			</v-tabs>
				
			<v-tabs-items v-model="tab" style="background: transparent;">
				<v-tab-item style="flex: 1;">
					<v-card flat>
						
						<v-form
							autocomplete="newpassword"
							ref="generalForm"
							>
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Basic</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="Name" 
											autocomplete="newpassword" 
											label="Project Name" 
											hint="A descriptive name for the this project."
											class="e2e-project-editor-name"
											:rules="[
												ValidateRequiredField
											]"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermProjectsCanPush()"
											>
											</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-select
											v-model="StatusId"
											label="Status"
											auto-select-first
											hint="What status this project is in."
											:items="StatusItems"
											item-value="id"
											item-text="json.name"
											class="e2e-project-editor-status"
											:rules="[
												ValidateRequiredField
											]"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermProjectsCanPush()"
											>
										</v-select>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="LastModified"
											autocomplete="newpassword"
											label="Last Activity"
											hint="The last time this project was modified."
											value="July 24, 2019, 7:18 PM CDT"
											readonly
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Other Projects</div>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<ProjectSelectField
											:isDialogue="isDialogue"
											label="Parent Project"
											v-model="ParentId"
											:excludeIds="value ? [value.id]: []"
											:showDetails="false"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermProjectsCanPush()"
											/>
									</v-col>
								</v-row>
								
								<v-row v-if="!isMakingNew">
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle-1" style="font-weight: bold;">Child Projects</div>
										<ProjectList
											:showFilter="false"
											:showOnlyChildrenOfProjectId="$route.params.id"
											:isDialogue="isDialogue"
											:disabled="connectionStatus != 'Connected'"
											/>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Address</div>
									</v-col>
								</v-row>
								
								<v-row v-if="ParentProject && ParentProjectAddresesRecursive && ParentProjectAddresesRecursive.length > 0">
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle-1" style="font-weight:bold; font-size: 14px;">From Parent Projects:</div>
										<table>
											<tr v-for="(obj) in ParentProjectAddresesRecursive" :key="obj.id">
												<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">
													<span v-if="obj.label">{{obj.label}}:</span>
												</td>
												<td style="font-size: 14px;"><a :href="`https://www.google.com/maps/dir/current+location/${obj.value.replace(/[\r\n\x0B\x0C\u0085\u2028\u2029\/]+/g, ' ')}`" target="_blank">{{obj.value}}</a></td>
											</tr>
										</table>
									</v-col>
								</v-row>
								
								<AddressEditRowArrayAdapter
									v-model="Addresses"
									:disabled="connectionStatus != 'Connected'"
									:readonly="!PermProjectsCanPush()"
									/>
								
								
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Companies</div>
									</v-col>
								</v-row>
								
								<v-row v-if="ParentProject && ParentProjectCompaniesRecursive && ParentProjectCompaniesRecursive.length > 0">
									
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle-1" style="font-weight:bold; font-size: 14px;">From Parent Projects:</div>
										<table>
											<tr v-for="(obj) in ParentProjectCompaniesRecursive" :key="obj.id">
												<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">
													<span v-if="obj.label">{{obj.label}}:</span>
												</td>
												<td style="font-size: 14px;">
													<router-link
														:to="`/section/companies/${obj.value}`"
														>
														{{CompanyNameForId(obj.value)}}
													</router-link>
												</td>
											</tr>
										</table>
									</v-col>
								</v-row>
								
								<LabeledCompanyEditRowArrayAdapter
									v-model="Companies"
									:disabled="connectionStatus != 'Connected'"
									:readonly="!PermProjectsCanPush()"
									/>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Contacts</div>
									</v-col>
								</v-row>
								
								<v-row v-if="ParentProject && ParentProjectContactsRecursive && ParentProjectContactsRecursive.length > 0">
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle-1" style="font-weight:bold; font-size: 14px;">From Parent Projects:</div>
										<table>
											<tr v-for="(obj) in ParentProjectContactsRecursive" :key="obj.id">
												<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">{{obj.label}}:</td>
												<td style="font-size: 14px;">
													<router-link
														:to="`/section/contacts/${obj.value}`"
														>
														{{ContactNameForId(obj.value)}}
													</router-link>
												</td>
											</tr>
										</table>
									</v-col>
								</v-row>
								
								<LabeledContactEditRowArrayAdapter 
									v-model="Contacts"
									:disabled="connectionStatus != 'Connected'"
									:readonly="!PermProjectsCanPush()"
									/>
								
								
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Advanced</div>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="Id"
											readonly="readonly"
											label="Unique ID"
											hint="The id of this project."
											>
										</v-text-field>
									</v-col>
								</v-row>
								
							</v-container>
						</v-form>

					</v-card>
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<v-card flat>
						<v-form ref="scheduleForm">
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-alert
											v-if="ForceAssignmentsToUseProjectSchedule"
											border="top"
											colored-border
											elevation="2"
											style="padding-bottom: 10px;"
											type="info"
											>
											Assignments on this project will be changed as well. If you don't want this, you can change this in the settings tab above.
										</v-alert>
										<v-alert
											v-if="ProjectChildProjectsOfId(value.id).length > 0"
											border="top"
											colored-border
											elevation="2"
											style="padding-bottom: 10px;"
											type="info"
											>
											This tab doesn't show child project schedules.
										</v-alert>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Start</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle-1" style="font-weight: bold;">Date</div>
										
										
										
										<v-switch
											v-model="HasStartISO8601"
											:label="`Scheduled Start`"
											class="e2e-project-schedule-has-start"
											:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush()"
											>
										</v-switch>
										
										<div v-if="HasStartISO8601">
											<p>
												<v-date-picker
													v-model="StartDateLocal"
													:elevation="1"
													class="e2e-date-picker-start"
													:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush()"
													>
												</v-date-picker>
											</p>
											
											
											<div class="subtitle-1" style="font-weight: bold;">Time</div>
											
											<v-radio-group
												v-model="StartTimeMode"
												:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush()"
												>
												<v-radio
													key="none"
													label="No Time"
													value="none"
												></v-radio>
												<v-radio
													key="morning-first-thing"
													label="Morning First Thing"
													value="morning-first-thing"
												></v-radio>
												<v-radio
													key="morning-second-thing"
													label="Morning Second Thing"
													value="morning-second-thing"
												></v-radio>
												<v-radio
													key="afternoon-first-thing"
													label="Afternoon First Thing"
													value="afternoon-first-thing"
												></v-radio>
												<v-radio
													key="afternoon-second-thing"
													label="Afternoon Second Thing"
													value="afternoon-second-thing"
												></v-radio>
												<v-radio
													key="time"
													label="Specific Time"
													value="time"
												></v-radio>
											</v-radio-group>
											
											
											
											<p v-if="StartTimeMode == 'time'">
												<v-time-picker
													:ampm-in-title="true"
													:allowed-minutes="TimePicker5MinuteStep"
													v-model="StartTimeLocal"
													:elevation="1"
													:disabled="connectionStatus != 'Connected'"
													:readonly="!PermProjectsCanPush()"
													>
												</v-time-picker>
											</p>
										
										</div>
										
										
										
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title" style="font-weight: bold;">End</div>
										
										<div class="subtitle-1" style="font-weight: bold;">Date</div>
										<v-switch
											v-model="HasEndISO8601"
											:label="`Scheduled End`"
											class="e2e-project-schedule-has-end"
											:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush()"
											>
										</v-switch>
										
										<div v-if="HasEndISO8601">
											<p>
												<v-date-picker
													v-model="EndDateLocal"
													:elevation="1"
													class="e2e-date-picker-end"
													:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush()"
													>
												</v-date-picker>
											</p>
											
											<v-radio-group
												v-model="EndTimeMode"
												:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush()"
												>
												<v-radio
													key="none"
													label="No Time"
													value="none"
												></v-radio>
												<v-radio
													key="time"
													label="Specific Time"
													value="time"
												></v-radio>
											</v-radio-group>
											
											<p v-if="EndTimeMode == 'time'">
												<v-time-picker
													:ampm-in-title="true"
													:allowed-minutes="TimePicker5MinuteStep"
													v-model="EndTimeLocal"
													:elevation="1"
													:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush()"
													>
												</v-time-picker>
											</p>
										</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<!-- <div class="title" style="font-weight: bold;">Push to Children</div> -->
										<div>
											<v-btn 
												@click="PushScheduleToChildren"
												:disabled="ProjectChildProjectsOfId(value.id).length == 0 || connectionStatus != 'Connected' || !PermProjectsCanPush()"
												>
												Push Schedule to Children
											</v-btn>
										</div>
									</v-col>
								</v-row>
								
								
							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<NoteList
						:showAddQuickNote="true"
						:showOnlyProjectId="$route.params.id"
						:showChildrenOfProjectIdAsWell="true"
						:dense="false"
						:isDialogue="isDialogue"
						:rootProject="value"
						:disabled="connectionStatus != 'Connected'"
						/>
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<AssignmentsList
						:showOnlyProjectId="$route.params.id"
						:showChildrenOfProjectIdAsWell="true"
						:focusIsAgent="true"
						:isDialogue="isDialogue"
						:rootProject="value"
						:disabled="connectionStatus != 'Connected'"
						/>
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<MaterialsList
						:showOnlyProjectId="$route.params.id"
						:showChildrenOfProjectIdAsWell="true"
						:prefillProjectId="$route.params.id"
						:isDialogue="isDialogue"
						:openEntryOnClick="false"
						:rootProject="value"
						:disabled="connectionStatus != 'Connected'"
						/>
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<v-card flat>
						<v-container>
							<v-row>
								<v-col cols="12" sm="10" offset-sm="1">
									<v-btn
										color="primary"
										text
										@click="StartTravel"
										:disabled="connectionStatus != 'Connected' || !PermLabourCanPushSelf()"
										>
										<v-icon left>commute</v-icon>
										Start Travel
									</v-btn>
									<v-btn
										color="primary"
										text
										@click="StartOnSite"
										:disabled="connectionStatus != 'Connected' || !PermLabourCanPushSelf()"
										>
										<v-icon left>build</v-icon>
										Start On-Site
									</v-btn>
									<v-btn
										color="primary"
										text
										@click="StartRemote"
										:disabled="connectionStatus != 'Connected' || !PermLabourCanPushSelf()"
										>
										<v-icon left>business</v-icon>
										Start Remote
									</v-btn>
									<v-btn
										color="primary"
										text
										:disabled="connectionStatus != 'Connected' || !PermLabourCanPushSelf() || !PermCRMLabourManualEntries()"
										@click="DialoguesOpen({
											name: 'ModifyLabourDialogue',
											state: {
												json: {
													projectId: $route.params.id,
													agentId: CurrentDPAgentId(),
													typeId: LabourTypeDefaultBillableTypeId(),
												}
											}
										})"
										>
										<v-icon left>add</v-icon>
										Manual Entry
									</v-btn>
									
								</v-col>
							</v-row>
						</v-container>
						<LabourList
							:showOnlyProjectId="$route.params.id"
							:showChildrenOfProjectIdAsWell="true"
							:prefillProjectId="$route.params.id"
							focusIsAgent="true"
							:openEntryOnClick="false"
							:isReverseSort="true"
							:isDialogue="isDialogue"
							:rootProject="value"
							:disabled="connectionStatus != 'Connected'"
							/>
					</v-card>
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<v-card flat>
						
						<v-form
							autocomplete="newpassword"
							ref="settingsForm"
							>
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-alert
											v-if="ProjectChildProjectsOfId(value.id).length > 0"
											border="top"
											colored-border
											elevation="2"
											style="padding-bottom: 10px;"
											type="info"
											>
											This tab does not show any settings from child projects.
										</v-alert>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Project Wide Settings</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle-1" style="font-weight: bold;">Labour</div>
										<v-switch
											v-model="ForceLabourAsExtra"
											:label="`Force Labour to be Extra`"
											hint="If you turn this on from off, all existing labour on this project will be marked as extra! Be sure you want to do this."
											:persistent-hint="true"
											:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush()"
											>
										</v-switch>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle-1" style="font-weight: bold;">Assignments</div>
										<v-switch
											v-model="ForceAssignmentsToUseProjectSchedule"
											:label="`Force Assignments to Use Project Schedule`"
											hint="If you turn this on from off, all existing assignments on this project will switch to the project schedule! Be sure you want to do this."
											:persistent-hint="true"
											:disabled="connectionStatus != 'Connected' || !PermProjectsCanPush()"
											>
										</v-switch>
									</v-col>
								</v-row>
								<!-- <v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div>
											<v-btn 
												@click="PushSettingsToChildren"
												:disabled="ProjectChildProjectsOfId(value.id).length == 0"
												>
												Push Settings to Children
											</v-btn>
										</div>
									</v-col>
								</v-row> -->
							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>
			</v-tabs-items>
		</div>
		<AddAssignmentDialogue2 
				v-model="addAssignmentModel"
				:isOpen="addAssignmentOpen"
				@Save="SaveAddAssignment"
				@Cancel="CancelAddAssignment"
				ref="addAssignmentDialogue"
				/>
		<v-footer
			v-if="showFooter"
			color="#747389"
			class="white--text"
			app
			inset>
			<v-btn
				color="white"
				text
				rounded
				:disabled="!value || connectionStatus != 'Connected' || !PermProjectsCanDelete()"
				@click="DialoguesOpen({ 
					name: 'DeleteProjectDialogue', 
					state: {
							redirectToIndex: true,
							id: value.id,
						}
					})"
				>
				<v-icon left>delete</v-icon>
				Delete
			</v-btn>
			
			<v-spacer />
			
			<AddMenuButton
				:disabled="!value || connectionStatus != 'Connected'"
				>
				<v-list-item
					@click="DialoguesOpen({ 
						name: 'AddProjectNoteDialogue', 
						state: { json: { projectId: $route.params.id }} 
						})"
					:disabled="connectionStatus != 'Connected' || !PermProjectNotesCanPush()"
					>
					<v-list-item-icon>
						<v-icon>note_add</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Add Note…</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item
					@click="OpenAddAssignment()"
					:disabled="connectionStatus != 'Connected' || !PermAssignmentCanPush()"
					>
					<v-list-item-icon>
						<v-icon>local_shipping</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Add Assignment…</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item
					@click="DialoguesOpen({ 
						name: 'ModifyMaterialDialogue', 
						state: { json: { projectId: $route.params.id }} 
						})"
					:disabled="connectionStatus != 'Connected' || !PermMaterialsCanPush()"
					>
					<v-list-item-icon>
						<v-icon>local_grocery_store</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Add Material…</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item
					@click="DialoguesOpen({ 
						name: 'ModifyLabourDialogue',
						state: { 
							json: { 
								projectId: $route.params.id,
								agentId: CurrentDPAgentId(),
								typeId: LabourTypeDefaultBillableTypeId(),
							}} 
						})"
					:disabled="connectionStatus != 'Connected' || !PermLabourCanPush()"
					>
					<v-list-item-icon>
						<v-icon>timelapse</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Add Labour…</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</AddMenuButton>
		</v-footer>
		
	</div>
</template>
<script lang="ts">
import Dialogues from '@/Utility/Dialogues';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import LabeledCompanyEditRowArrayAdapter from '@/Components/Rows/LabeledCompanyEditRowArrayAdapter.vue';
import LabeledContactEditRowArrayAdapter from '@/Components/Rows/LabeledContactEditRowArrayAdapter.vue';
import ProjectNoteCard from '@/Components/Cards/ProjectNote/ProjectNoteCard.vue';
import AssignmentsList from '@/Components/Lists/AssignmentsList.vue';
import MaterialsList from '@/Components/Lists/MaterialsList.vue';
import LabourList from '@/Components/Lists/LabourList.vue';
import NoteList from '@/Components/Lists/NoteList.vue';
import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import AddressEditRowArrayAdapter from '@/Components/Rows/AddressEditRowArrayAdapter.vue';
import _ from 'lodash';
import { DateTime } from 'luxon';
import EditorBase, { IBreadcrumb, VForm } from './EditorBase'; 
import { IProject, Project } from '@/Data/CRM/Project/Project';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import TimePicker5MinuteStep from '@/Utility/TimePicker5MinuteStep';
import { Contact } from '@/Data/CRM/Contact/Contact';
import { Company } from '@/Data/CRM/Company/Company';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import { BillingContacts, IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { Agent } from '@/Data/CRM/Agent/Agent';
import ProjectList from '@/Components/Lists/ProjectList.vue';
import { LabourType } from '@/Data/CRM/LabourType/LabourType';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import AddAssignmentDialogue2 from '@/Components/Dialogues2/Assignments/AddAssignmentDialogue2.vue';
import CRMViews from '@/Permissions/CRMViews';
import { ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { Material } from '@/Data/CRM/Material/Material';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { IProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';
import { IAddress } from '@/Data/Models/Address/Address';
import { ILabeledCompanyId } from '@/Data/Models/LabeledCompanyId/LabeledCompanyId';
import { ILabeledContactId } from '@/Data/Models/LabeledContactId/LabeledContactId';

@Component({
	components: {
		AddMenuButton,
		AssignmentsList,
		ProjectNoteCard,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		LabeledCompanyEditRowArrayAdapter,
		LabeledContactEditRowArrayAdapter,
		MaterialsList,
		LabourList,
		AddressEditRowArrayAdapter,
		NoteList,
		ProjectList,
		ReloadButton,
		AddAssignmentDialogue2,
		NotificationBellButton,
	},
})
export default class ProjectEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: IProject | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	public $refs!: {
		generalForm: VForm,
		scheduleForm: VForm,
		settingsForm: VForm,
		addAssignmentDialogue: AddAssignmentDialogue2,
	};
	
	protected ValidateRequiredField = ValidateRequiredField;
	protected TimePicker5MinuteStep = TimePicker5MinuteStep;
	protected ContactNameForId = Contact.NameForId;
	protected CompanyNameForId = Company.NameForId;
	protected CurrentDPAgentId = Agent.LoggedInAgentId;
	protected DialoguesOpen = Dialogues.Open;
	protected ProjectChildProjectsOfId = Project.ChildProjectsOfId;
	protected LabourTypeDefaultBillableTypeId = LabourType.DefaultBillableTypeId;
	protected PermProjectsCanPush = Project.PermProjectsCanPush;
	protected PermContactsCanPush = Contact.PermContactsCanPush;
	protected PermAssignmentCanPush = Assignment.PermAssignmentCanPush;
	protected PermCRMViewProjectIndexMergeProjects = CRMViews.PermCRMViewProjectIndexMergeProjects;
	protected PermCRMReportProjectsPDF = Project.PermCRMReportProjectsPDF;
	protected PermCRMExportProjectsCSV = Project.PermCRMExportProjectsCSV;
	protected PermProjectNotesCanPush = ProjectNote.PermProjectNotesCanPush;
	protected PermMaterialsCanPush = Material.PermMaterialsCanPush;
	protected PermLabourCanPush = Labour.PermLabourCanPush;
	protected PermProjectsCanDelete = Project.PermProjectsCanDelete;
	protected PermAssignmentCanRequest = Assignment.PermAssignmentCanRequest;
	protected PermProjectNotesCanRequest = ProjectNote.PermProjectNotesCanRequest;
	protected PermMaterialsCanRequest = Material.PermMaterialsCanRequest;
	protected PermLabourCanRequest = Labour.PermLabourCanRequest;
	protected PermLabourCanPushSelf = Labour.PermLabourCanPushSelf;
	protected PermCRMLabourManualEntries = Labour.PermCRMLabourManualEntries;
	
	protected debounceId: ReturnType<typeof setTimeout> | null = null;
	
	protected addAssignmentModel: IAssignment | null = null;
	protected addAssignmentOpen = false;
	
	constructor() {
		super();
		
	}
	
	
	
	public GetValidatedForms(): VForm[] {
		return [
			this.$refs.generalForm as VForm,
			this.$refs.scheduleForm as VForm,
		];
	}
	
	protected GetTabNameToIndexMap(): Record<string, number> {
		return {
			General: 0,
			general: 0,
			Schedule: 1,
			schedule: 1,
			Notes: 2,
			notes: 2,
			Assignments: 3,
			assignments: 3,
			Materials: 4,
			materials: 4,
			Labour: 5,
			labour: 5,
			Settings: 6,
			settings: 6,
		};
	}
	
	protected get Id(): string | null {
		if (!this.value ||
			!this.value.id
			) {
			return null;
		}
		
		return this.value.id;
	}
	
	protected get Name(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.name
			) {
			return null;
		}
		
		return this.value.json.name;
	}
	
	protected set Name(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.name = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get ParentId(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.parentId
			) {
			return null;
		}
		
		return this.value.json.parentId;
	}
	
	protected set ParentId(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.parentId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get StatusId(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.statusId
			) {
			return null;
		}
		
		return this.value.json.statusId;
	}
	
	protected set StatusId(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.statusId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get StatusItems(): IProjectStatus[] {
		
		const filtered = _.filter(
			this.$store.state.Database.projectStatus,
			(o: IProjectStatus) => { // eslint-disable-line @typescript-eslint/no-unused-vars
				return true;
			});
		
		const sorted = _.sortBy(filtered, (o: IProjectStatus) => {
			return o.json.name;
		});
		
		
		return sorted;
		
		
	}
	
	protected get LastModified(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.lastModifiedISO8601 ||
			IsNullOrEmpty(this.value.lastModifiedISO8601)
			) {
			return null;
		}
		
		const date = DateTime.fromISO(this.value.lastModifiedISO8601);
		const dateStr = date.toLocaleString(DateTime.DATETIME_FULL);
		
		return `${dateStr} by ${this.ProjectLastModifiedWho ? this.ProjectLastModifiedWho.fullName : '?'}`;
	}
	
	protected get ProjectLastModifiedWho(): IBillingContacts | null {
		if (!this.value ||
			!this.value.json) {
			return null;
		}
		
		const lastModifiedBillingId = this.value.json.lastModifiedBillingId;
		if (!lastModifiedBillingId || IsNullOrEmpty(lastModifiedBillingId)) {
			return null;
		}
		
		return BillingContacts.ForId(lastModifiedBillingId);
	}
	
	protected get Addresses(): IAddress[] {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.addresses
			) {
			return [];
		}
		
		return this.value.json.addresses;
	}
	
	protected set Addresses(val: IAddress[]) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.addresses = val == null ? [] : val;
		this.SignalChanged();
	}
	
	
	protected get Companies(): ILabeledCompanyId[] {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.companies
			) {
			return [];
		}
		
		return this.value.json.companies;
	}
	
	protected set Companies(val: ILabeledCompanyId[]) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.companies = val == null ? [] : val;
		this.SignalChanged();
	}
	
	protected get Contacts(): ILabeledContactId[] {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.contacts
			) {
			return [];
		}
		
		return this.value.json.contacts;
	}
	
	protected set Contacts(val: ILabeledContactId[]) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.contacts = val == null ? [] : val;
		this.SignalChanged();
	}
	
	
	protected get ParentProject(): IProject | null {
		
		if (this.value === null) {
			return null;
		}
		if (!this.value ||
			!this.value.json ||
			!this.value.json.parentId) {
			return null;
		}
		
		const project = Project.ForId(this.value.json.parentId);
		if (!project) {
			return null;
		}
		
		return project;
	}
	
	protected get ParentProjectAddresesRecursive(): IAddress[] {
		
		let addreses: IAddress[] = [];
		const addedProjectIds: string[] = [];
		
		const fn = (projectId: string | null) => {
			
			if (!projectId) {
				return;
			}
			
			// we don't want to add the same project twice to avoid infinite loops
			if (addedProjectIds.indexOf(projectId) !== -1) {
				return;
			}
			
			const project = Project.ForId(projectId);
			if (!project) {
				return;
			}
			
			
			addreses = _.concat(addreses, project.json.addresses);
			addedProjectIds.push(projectId);
			
			fn(project.json.parentId);
			
		};
		
		const parentProjectId = this.value?.json.parentId;
		
		if (parentProjectId) {
			fn(parentProjectId);
		}
		
		// Remove empty addreses.
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		_.remove(addreses, (value, index, array) => {
			if (value.value === '') {
				return true;
			}
			if (!value) {
				return true;
			}
			if (IsNullOrEmpty(value.value)) {
				return true;
			}
			return false;
		});
		
		return addreses;
	}
	
	
	protected get ParentProjectCompaniesRecursive(): ILabeledCompanyId[] {
		
		let companies: ILabeledCompanyId[] = [];
		const addedProjectIds: string[] = [];
		
		const fn = (projectId: string | null) => {
			
			if (!projectId) {
				return;
			}
			
			// we don't want to add the same project twice to avoid infinite loops
			if (addedProjectIds.indexOf(projectId) !== -1) {
				return;
			}
			
			const project = Project.ForId(projectId);
			if (!project) {
				return;
			}
			
			
			companies = _.concat(companies, project.json.companies);
			addedProjectIds.push(projectId);
			
			fn(project.json.parentId);
			
		};
		
		const parentProjectId = this.value?.json.parentId;
		
		if (parentProjectId) {
			fn(parentProjectId);
		}
		
		// Remove empty addreses.
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		_.remove(companies, (value, index, array) => {
			if (value.value === '') {
				return true;
			}
			if (!value) {
				return true;
			}
			if (IsNullOrEmpty(value.value)) {
				return true;
			}
			return false;
		});
		
		return companies;
	}
	
	protected get ParentProjectContactsRecursive(): ILabeledContactId[] {
		
		let contacts: ILabeledContactId[] = [];
		const addedProjectIds: string[] = [];
		
		const fn = (projectId: string | null) => {
			
			if (!projectId) {
				return;
			}
			
			// we don't want to add the same project twice to avoid infinite loops
			if (addedProjectIds.indexOf(projectId) !== -1) {
				return;
			}
			
			const project = Project.ForId(projectId);
			if (!project) {
				return;
			}
			
			
			contacts = _.concat(contacts, project.json.contacts);
			addedProjectIds.push(projectId);
			
			fn(project.json.parentId);
			
		};
		
		const parentProjectId = this.value?.json.parentId;
		
		if (parentProjectId) {
			fn(parentProjectId);
		}
		
		// Remove empty addreses.
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		_.remove(contacts, (value, index, array) => {
			if (value.value === '') {
				return true;
			}
			if (!value) {
				return true;
			}
			if (IsNullOrEmpty(value.value)) {
				return true;
			}
			return false;
		});
		
		
		return contacts;
	}
	
	
	
	protected get EndTimeLocal(): string | null {
		
		if (!this.value) {
			return null;
		}
		
		const json = this.value.json;
		if (!json) {
			return null;
		}
		
		const iso8601 = json.endISO8601;
		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}
		
		const utc = DateTime.fromISO(iso8601);
		if (!utc) {
			return null;
		}
		
		const local = utc.toLocal();
		if (!local) {
			return null;
		}
		
		return local.toFormat('HH:mm');
	}
	
	protected set EndTimeLocal(val: string | null) {
		
		if (!this.value) {
			return;
		}
		
		let validatedVar = null;
		
		do {
			if (!val) {
				break;
			}
			if (!this.value) {
				break;
			}
			
			const dbISO8601 = this.value.json.endISO8601;
			
			let dbUtc = null;
			
			if (dbISO8601) {
				dbUtc = DateTime.fromISO(dbISO8601);
			} else {
				dbUtc = DateTime.utc();
			}
			
			const dbLocal = dbUtc.toLocal();
			
			const valDateLocal = DateTime.fromFormat(val, 'HH:mm');
			const dbLocalMod = dbLocal.set({
				hour: valDateLocal.hour,
				minute: valDateLocal.minute,
			});
			
			const dbUtcMod = dbLocalMod.toUTC();
			validatedVar = dbUtcMod.toISO();
			
		} while (false);
		
		//console.log('set EndDateLocal', val, validatedVar);
		
		this.value.json.endISO8601 = validatedVar;
		
		if (this.ForceAssignmentsToUseProjectSchedule) {
			this.UpdateAssignmentsToProjectSchedule();
		}
		
		this.SignalChanged();
		
	}
	
	
	protected get EndDateLocal(): string | null {
		
		if (!this.value) {
			return null;
		}
		
		const json = this.value.json;
		if (!json) {
			return null;
		}
		
		const iso8601 = json.endISO8601;
		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}
		
		const utc = DateTime.fromISO(iso8601);
		if (!utc) {
			return null;
		}
		
		const local = utc.toLocal();
		if (!local) {
			return null;
		}
		
		return local.toFormat('yyyy-MM-dd');
	}
	
	
	
	protected set EndDateLocal(val: string | null) {
		
		if (!this.value) {
			return;
		}
		
		let validatedVar = null;
		
		do {
			if (!val) {
				break;
			}
			if (!this.value) {
				break;
			}
			
			
			const dbISO8601 = this.value.json.endISO8601;
			
			let dbUtc = null;
			
			if (dbISO8601) {
				dbUtc = DateTime.fromISO(dbISO8601);
			} else {
				dbUtc = DateTime.utc();
			}
			
			const dbLocal = dbUtc.toLocal();
			
			const valDateLocal = DateTime.fromFormat(val, 'yyyy-MM-dd');
			const dbLocalMod = dbLocal.set({
				year: valDateLocal.year,
				month: valDateLocal.month,
				day: valDateLocal.day,
			});
			
			const dbUtcMod = dbLocalMod.toUTC();
			validatedVar = dbUtcMod.toISO();
			
		} while (false);
		
		//console.log('set EndDateLocal', val, validatedVar);
		
		this.value.json.endISO8601 = validatedVar;
		
		if (this.ForceAssignmentsToUseProjectSchedule) {
			this.UpdateAssignmentsToProjectSchedule();
		}
		
		this.SignalChanged();
		
		
	}
	
	
	
	
	
	protected get StartTimeLocal(): string | null {
		
		if (!this.value) {
			return null;
		}
		
		const json = this.value.json;
		if (!json) {
			return null;
		}
		
		const iso8601 = json.startISO8601;
		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}
		
		const utc = DateTime.fromISO(iso8601);
		if (!utc) {
			return null;
		}
		
		const local = utc.toLocal();
		if (!local) {
			return null;
		}
		
		return local.toFormat('HH:mm');
	}
	
	protected set StartTimeLocal(val: string | null) {
		
		console.log('set StartTimeLocal', val);
		
		if (!this.value) {
			return;
		}
		
		let validatedVar = null;
		
		do {
			if (!val ||
				!this.value
				) {
				validatedVar = null;
				break;
			}
			
			
			const dbISO8601 = this.value.json.startISO8601;
			
			let dbUtc = null;
			
			if (dbISO8601) {
				dbUtc = DateTime.fromISO(dbISO8601);
			} else {
				dbUtc = DateTime.utc();
			}
			
			const dbLocal = dbUtc.toLocal();
			
			const valDateLocal = DateTime.fromFormat(val, 'HH:mm');
			const dbLocalMod = dbLocal.set({
				hour: valDateLocal.hour,
				minute: valDateLocal.minute,
			});
			
			const dbUtcMod = dbLocalMod.toUTC();
			validatedVar = dbUtcMod.toISO();
			
		} while (false);
		
		
		
		
		this.value.json.startISO8601 = validatedVar;
		
		if (this.ForceAssignmentsToUseProjectSchedule) {
			this.UpdateAssignmentsToProjectSchedule();
		}
		
		this.SignalChanged();
		
		
	}
	
	
	
	
	
	
	
	protected get StartDateLocal(): string | null {
		
		if (!this.value) {
			return null;
		}
		
		const json = this.value.json;
		if (!json) {
			return null;
		}
		
		const iso8601 = json.startISO8601;
		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}
		
		const utc = DateTime.fromISO(iso8601);
		if (!utc) {
			return null;
		}
		
		const local = utc.toLocal();
		if (!local) {
			return null;
		}
		
		return local.toFormat('yyyy-MM-dd');
	}
	
	
	
	protected set StartDateLocal(val: string | null) {
		
		
		
		if (!this.value) {
			return;
		}
		
		let validatedVar = null;
		
		do {
			if (!val ||
				!this.value) {
				validatedVar = null;
				break;
			}
			
			
			const dbISO8601 = this.value.json.startISO8601;
			
			let dbUtc = null;
			
			if (dbISO8601) {
				dbUtc = DateTime.fromISO(dbISO8601);
			} else {
				dbUtc = DateTime.utc();
			}
			
			const dbLocal = dbUtc.toLocal();
			
			const valDateLocal = DateTime.fromFormat(val, 'yyyy-MM-dd');
			const dbLocalMod = dbLocal.set({
				year: valDateLocal.year,
				month: valDateLocal.month,
				day: valDateLocal.day,
			});
			
			const dbUtcMod = dbLocalMod.toUTC();
			validatedVar = dbUtcMod.toISO();
			
		} while (false);
		
		console.log('set StartDateLocal', val, validatedVar);
		
		
		this.value.json.startISO8601 = validatedVar;
		
		if (this.ForceAssignmentsToUseProjectSchedule) {
			this.UpdateAssignmentsToProjectSchedule();
		}
		
		this.SignalChanged();
		
		
		
	}
	
	
	protected get EndTimeMode(): 'none' | 'time' {
		
		if (!this.value) {
			return 'none';
		}
		
		const json = this.value.json;
		if (!json) {
			return 'none';
		}
		
		const timeMode = json.endTimeMode;
		if (IsNullOrEmpty(timeMode)) {
			return 'none';
		}
		
		return timeMode;
	}
	
	protected set EndTimeMode(val: 'none' | 'time') {
		
		if (!this.value) {
			return;
		}
		
		this.value.json.endTimeMode = val;
		
		// Update the other variables based on the mode change, notably the time.
		const dbISO8601 = this.value.json.endISO8601;
		if (dbISO8601 != null && !IsNullOrEmpty(dbISO8601)) {
			
			const dbUTC = DateTime.fromISO(dbISO8601);
			const dbLocal = dbUTC.toLocal();
			let mod;
			let modUtc;
			
			switch (val) {
				case 'none':
					
					mod = dbLocal.set({
						hour: 0,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					this.value.json.endISO8601 = modUtc.toISO();
					
					break;
				case 'time':
					
					break;
			}
			
			
		}
		
		if (this.ForceAssignmentsToUseProjectSchedule) {
			this.UpdateAssignmentsToProjectSchedule();
		}
		
		this.SignalChanged();
		
		
	}
	
	
	
	
	
	
	protected get StartTimeMode(): 'none' | 'morning-first-thing' | 'morning-second-thing' | 'afternoon-first-thing' | 'afternoon-second-thing' | 'time' {
		
		if (!this.value) {
			return 'none';
		}
		
		const json = this.value.json;
		if (!json) {
			return 'none';
		}
		
		const timeMode = json.startTimeMode;
		if (IsNullOrEmpty(timeMode)) {
			return 'none';
		}
		
		return timeMode;
	}
	
	protected set StartTimeMode(val: 'none' | 'morning-first-thing' | 'morning-second-thing' | 'afternoon-first-thing' | 'afternoon-second-thing' | 'time') {
		
		if (!this.value) {
			return;
		}
		
		this.value.json.startTimeMode = val;
		
		// Update the other variables based on the mode change, notably the time.
		const dbISO8601 = this.value.json.startISO8601;
		if (dbISO8601 != null && !IsNullOrEmpty(dbISO8601)) {
			
			const dbUTC = DateTime.fromISO(dbISO8601);
			const dbLocal = dbUTC.toLocal();
			let mod;
			let modUtc;
			
			switch (val) {
				case 'none':
					
					mod = dbLocal.set({
						hour: 0,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					this.value.json.startISO8601 = modUtc.toISO();
					
					break;
				case 'morning-first-thing':
					
					mod = dbLocal.set({
						hour: 8,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					this.value.json.startISO8601 = modUtc.toISO();
					
					break;
				case 'morning-second-thing':
					
					mod = dbLocal.set({
						hour: 10,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					this.value.json.startISO8601 = modUtc.toISO();
					
					break;
				case 'afternoon-first-thing':
					
					mod = dbLocal.set({
						hour: 13,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					this.value.json.startISO8601 = modUtc.toISO();
					
					break;
				case 'afternoon-second-thing':
					
					mod = dbLocal.set({
						hour: 15,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					this.value.json.startISO8601 = modUtc.toISO();
					
					break;
				case 'time':
					
					break;
			}
			
			
		}
		
		if (this.ForceAssignmentsToUseProjectSchedule) {
			this.UpdateAssignmentsToProjectSchedule();
		}
		
		this.SignalChanged();
		
		
		
	}
	
	
	
	protected get HasStartISO8601(): boolean {
		
		if (!this.value) {
			return false;
		}
		
		const json = this.value.json;
		if (!json) {
			return false;
		}
		
		const hasStart = json.hasStartISO8601;
		if (hasStart === undefined) {
			return false;
		}
		
		
		return hasStart;
	}
	
	protected set HasStartISO8601(val: boolean) {
		
		if (!this.value) {
			return;
		}
		
		this.value.json.hasStartISO8601 = val;
		if (!val) {
			this.value.json.startTimeMode = 'none';
			
			let d = DateTime.local();
			d = d.set({hour: 0, minute: 0, second: 0});
			d = d.toUTC();
			
			this.value.json.startISO8601 = d.toISO();
		}
		
		if (this.ForceAssignmentsToUseProjectSchedule) {
			this.UpdateAssignmentsToProjectSchedule();
		}
		
		
		
		this.SignalChanged();
	}
	
	
	
	protected get ForceLabourAsExtra(): boolean {
		
		if (!this.value) {
			return false;
		}
		
		const json = this.value.json;
		if (!json) {
			return false;
		}
		
		const forceLabourAsExtra = json.forceLabourAsExtra;
		if (forceLabourAsExtra === undefined || forceLabourAsExtra === null) {
			return false;
		}
		
		
		return forceLabourAsExtra;
	}
	
	protected set ForceLabourAsExtra(val: boolean) {
		
		if (!this.value ||
			!this.value.id) {
			return;
		}
		
		this.value.json.forceLabourAsExtra = val;
		
		// Update all existing labour to be extra.
		
		if (val) {
			const payload: Record<string, ILabour> = {};
			
			const labourEntries = Labour.ForProjectIds([this.value.id]);
			for (const entry of labourEntries) {
				if (!entry.id) {
					continue;
				}
				
				entry.json.isExtra = true;
				payload[entry.id] = entry;
				
			}
			
			Labour.UpdateIds(payload);
		}
		
		
		
		this.SignalChanged();
	}
	
	
	protected get ForceAssignmentsToUseProjectSchedule(): boolean {
		
		if (!this.value) {
			return false;
		}
		
		const json = this.value.json;
		if (!json) {
			return false;
		}
		
		const forceAssignmentsToUseProjectSchedule = json.forceAssignmentsToUseProjectSchedule;
		if (forceAssignmentsToUseProjectSchedule === undefined || forceAssignmentsToUseProjectSchedule === null) {
			return false;
		}
		
		
		return forceAssignmentsToUseProjectSchedule;
	}
	
	protected set ForceAssignmentsToUseProjectSchedule(val: boolean) {
		
		if (!this.value) {
			return;
		}
		
		this.value.json.forceAssignmentsToUseProjectSchedule = val;
		
		// Update all existing assignments to use the project schedule.
		
		// console.log('ForceAssignmentsToUseProjectSchedule', val);
		
		if (val) {
			this.UpdateAssignmentsToProjectSchedule();
		}
		
		
		
		this.SignalChanged();
	}
	
	protected UpdateAssignmentsToProjectSchedule(): void {
		
		// console.log('@@@');
		
		if (!this.value ||
			!this.value.id) {
			return;
		}
		
		
		
		// console.log('UpdateAssignmentsToProjectSchedule');
		
		const payload: Record<string, IAssignment> = {};
			
		const entries = Assignment.ForProjectIds([this.value.id]);
		for (const entry of entries) {
			if (!entry.id) {
				continue;
			}
			
			entry.json.startISO8601 = this.value.json.startISO8601;
			entry.json.hasEndISO8601 = this.value.json.hasEndISO8601;
			entry.json.endTimeMode = this.value.json.endTimeMode;
			entry.json.endISO8601 = this.value.json.endISO8601;
			entry.json.hasStartISO8601 = this.value.json.hasStartISO8601;
			entry.json.startTimeMode = this.value.json.startTimeMode;
			
			payload[entry.id] = entry;
			
		}
		
		Assignment.UpdateIds(payload);
		
	}
	
	
	
	
	
	
	
	
	
	
	
	protected get HasEndISO8601(): boolean {
		
		if (!this.value) {
			return false;
		}
		
		const json = this.value.json;
		if (!json) {
			return false;
		}
		
		const hasEnd = json.hasEndISO8601;
		if (hasEnd === undefined) {
			return false;
		}
		
		return hasEnd;
	}
	
	protected set HasEndISO8601(val: boolean) {
		
		if (!this.value) {
			return;
		}
		
		this.value.json.hasEndISO8601 = val;
		
		this.SignalChanged();
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	protected StartTravel(): void {
		if (!this.value ||
			!this.value.id
			) {
			return;
		}
		
		Project.StartTravelForId(this.value.id);
	}
	
	
	
	protected StartOnSite(): void {
		
		if (!this.value ||
			!this.value.id
			) {
			return;
		}
		
		Project.StartOnSiteForId(this.value.id);
	}
	
	
	protected StartRemote(): void {
		
		//console.debug('StartRemote()');
		
		if (!this.value ||
			!this.value.id
			) {
			return;
		}
		
		Project.StartRemoteForId(this.value.id);
	}
	
	protected PushScheduleToChildren(): void {
		console.log('PushScheduleToChildren', this.value);
		
		if (!this.value ||
			!this.value.id) {
			return;
		}
		
		const payload: Record<string, IProject> = {};
		
		const projects = Project.ChildProjectsOfId(this.value.id);
		for (const project of projects) {
			if (!project.id) {
				continue;
			}
			
			project.lastModifiedISO8601 = DateTime.utc().toISO();
			project.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
			
			project.json.hasStartISO8601 = this.value.json.hasStartISO8601;
			project.json.startTimeMode = this.value.json.startTimeMode;
			project.json.startISO8601 = this.value.json.startISO8601;
			
			project.json.hasEndISO8601 = this.value.json.hasEndISO8601;
			project.json.endTimeMode = this.value.json.endTimeMode;
			project.json.endISO8601 = this.value.json.endISO8601;
			
			payload[project.id] = project;
		}
		
		console.log('payload', payload);
		
		Project.UpdateIds(payload);
		
	}
	
	// protected PushSettingsToChildren() {
	// 	console.log('PushSettingsToChildren');
	// }
	
	
	
	
	
	
	
	
	
	
	
	
	protected DoPrint(): void {
		
		Dialogues.Open({ 
			name: 'ProjectReportDialogue', 
			state: {
				allLoadedProjects: false,
				specificProjects: [ this.value?.id ],
			},
		});
		
	}
	
	protected OpenAddAssignment(): void {
		
		//console.debug('OpenAddAssignment');
		
		this.addAssignmentModel = Assignment.GetEmpty();
		this.addAssignmentModel.json.projectId = this.$route.params.id;
		this.addAssignmentOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.addAssignmentDialogue) {
				this.$refs.addAssignmentDialogue.SwitchToTabFromRoute();
			}
		});
		
	}
	
	protected CancelAddAssignment(): void {
		
		//console.debug('CancelAddAssignment');
		
		this.addAssignmentOpen = false;
		
	}
	
	protected SaveAddAssignment(): void {
		
		//console.debug('SaveAddAssignment');
		
		this.addAssignmentOpen = false;
		
		if (!this.addAssignmentModel ||
			!this.addAssignmentModel.id) {
			return;
		}
		
		const payload: Record<string, IAssignment> = {};
		payload[this.addAssignmentModel.id] = this.addAssignmentModel;
		Assignment.UpdateIds(payload);
		
		this.$router.push(`/section/assignments/${this.addAssignmentModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		
	}
	
	
	
	
	
	
	
	
	
	protected SignalChanged(): void {
		
		// Debounce
		
		if (this.debounceId) {
			clearTimeout(this.debounceId);
			this.debounceId = null;
		}
		
		this.debounceId = setTimeout(() => {
			this.$emit('input', this.value);
		}, 250);
	}
	
	protected OnlineHelpFiles(): void {
		//console.log('OpenOnlineHelp()');
		
		window.open('https://www.dispatchpulse.com/Support', '_blank');
	}

}


Vue.component('ProjectEditor', ProjectEditor);

</script>