export interface ApiResponse<T>{
    statusCode: number;
    isSuccess: boolean;
    errorMasseges: string[];
    result: any;
}