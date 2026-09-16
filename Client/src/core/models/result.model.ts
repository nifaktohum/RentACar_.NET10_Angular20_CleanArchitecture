export interface Result<T> {
  data?: T
  errorMessages?: string[] | null; 
  isSuccessful: boolean
  statusCode: number
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}


export interface ApiResponse<T> extends Result<T> { }