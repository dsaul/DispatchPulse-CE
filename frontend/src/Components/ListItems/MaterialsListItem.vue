<template>
	<v-list-item>
		<v-list-item-avatar>
			<v-icon>local_grocery_store</v-icon>
		</v-list-item-avatar>

		<v-list-item-content @click="ClickEntry">
			<v-list-item-title style="white-space: normal;">{{value.json.quantity}} {{ value.json.quantityUnit || '&#x274C;'}} {{ProductName}}</v-list-item-title>
			<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/">
				
				<v-chip 
					v-if="DateUsed"
					label
					outlined
					small
					style="margin-right: 5px;"
					>
					<v-avatar left>
						<v-icon small>calendar_today</v-icon>
					</v-avatar>
					{{DateUsed}}
				</v-chip>
				<v-chip 
					v-if="Location"
					label
					outlined
					small
					style="margin-right: 5px;"
					>
					{{Location}}
				</v-chip>
				<v-chip 
					v-if="IsExtra"
					label
					outlined
					small
					style="margin-right: 5px;"
					>
					Extra
				</v-chip>
				<v-chip 
					v-if="IsBilled"
					label
					outlined
					small
					style="margin-right: 5px;"
					>
					Billed
				</v-chip>
				<v-chip 
					v-if="Notes"
					label
					outlined
					small
					style="margin-right: 5px;"
					>
					{{Notes}}
				</v-chip>
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
		</v-list-item-content>

		<v-list-item-action v-if="showMenuButton">
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
						:disabled="isDialogue || disabled || !PermMaterialsCanPush()"
						@click="$emit('OpenEntry', value.id)"
						>
						<v-list-item-icon>
							<v-icon>edit</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Edit…</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="$emit('DeleteEntry', value.id)"
						:disabled="disabled || !PermMaterialsCanDelete()"
						>
						<v-list-item-icon>
							<v-icon>delete</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Delete…</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</v-list>
			</v-menu>
		</v-list-item-action>
	</v-list-item>


</template>

<script lang="ts">

import { Component, Prop } from 'vue-property-decorator';
import ListItemBase from './ListItemBase';
import { DateTime } from 'luxon';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { Product } from '@/Data/CRM/Product/Product';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import SignalRConnection from '@/RPC/SignalRConnection';
import { IMaterial, Material } from '@/Data/CRM/Material/Material';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	
})
export default class MaterialsListItem extends ListItemBase {
	
	@Prop({ default: null }) declare public readonly value: IMaterial;
	@Prop({ default: null }) public readonly rootProject!: IProject;
	
	protected PermMaterialsCanPush = Material.PermMaterialsCanPush;
	protected PermMaterialsCanRequest = Material.PermMaterialsCanRequest;
	protected PermMaterialsCanDelete = Material.PermMaterialsCanDelete;
	protected ProjectNameForId = Project.NameForId;
	
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
				
				if (null != this.value.json.productId && !IsNullOrEmpty(this.value.json.productId)) {
					
					const product = Product.ForId(this.value.json.productId);
					if (null == product && Product.PermProductsCanRequest()) {
						
						const rtr = Product.FetchForId(this.value.json.productId);
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
	
	
	
	
	protected get DateUsed(): string | null {
		
		console.log('DateUsed', this.value);
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.dateUsedISO8601
			) {
				return null;
			}
		
		const d =  DateTime. fromISO(this.value.json.dateUsedISO8601 );
		return d.toLocaleString(DateTime.DATE_FULL);
	}
	
	protected get Location(): string | null {
		if (!this.value || 
			!this.value.json || 
			!this.value.json.productId || 
			IsNullOrEmpty(this.value.json.location)
			) {
			return null;
		}
		
		return this.value.json.location;
	}
	
	protected get IsExtra(): boolean {
		if (!this.value || 
			!this.value.json || 
			!this.value.json.productId || 
			this.value.json.isExtra === null
			) {
			return false;
		}
		
		return this.value.json.isExtra;
	}
	
	protected get IsBilled(): boolean {
		if (!this.value || 
			!this.value.json || 
			!this.value.json.productId || 
			this.value.json.isBilled === null
			) {
			return false;
		}
		
		return this.value.json.isBilled;
	}
	
	protected get Notes(): string | null {
		if (!this.value || 
			!this.value.json || 
			!this.value.json.productId || 
			IsNullOrEmpty(this.value.json.notes)
			) {
			return null;
		}
		
		return this.value.json.notes;
	}
	
	
	protected get ProductName(): string | null {
		
		//console.debug('ProductName', this.value);
		
		if (!this.value || 
			!this.value.json || 
			!this.value.json.productId || 
			IsNullOrEmpty(this.value.json.productId)
			) {
			return null;
		}
		
		
		//console.debug('ProductName');
		
		return Product.NameForId(this.value.json.productId);
		
	}
	
	protected ClickEntry(): void {
		if (this.value && this.value.json && !IsNullOrEmpty(this.value.id)) {
			this.$emit('ClickEntry', this.value.id);
		}
	}
	
}

</script>