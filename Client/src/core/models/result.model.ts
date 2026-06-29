export interface Result<T> {
  data?: T
  errorMessages?: string[] | null; 
  isSuccessful: boolean
  statusCode: number
}