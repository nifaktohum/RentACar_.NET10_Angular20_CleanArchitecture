import { EntityModel } from "./entity.model";

export interface BranchModel extends EntityModel {
  name: string;        
  address: AddressModel;
  isActive: boolean;
}


export interface GetBranchesResponse {
  id?: string;
  items: BranchModel[];
  totalCount: number;
}

export type AddressModel = {
  city: string;        
  district: string;    
  fullAddress: string; 
  phone1: string;      
  phone2?: string | null;     
  email: string;       
}