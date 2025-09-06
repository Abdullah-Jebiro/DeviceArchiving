import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { DevicesDto, DeviceDto, CheckDuplicateDto, DuplicateCheckResponse, DeviceUploadDto } from '../models/device.model';
import { CreateDeviceDto } from '../models/create-device.model';
import { BaseResponse, UpdateDeviceDto } from '../models/update-device.model';

@Injectable({
  providedIn: 'root'
})
export class DeviceService {
  private readonly apiUrl: string = `${environment.apiBaseUrl}/api/Devices`;

  constructor(private readonly http: HttpClient) { }



  getAll(): Observable<DevicesDto[]> {
    return this.http.get<DevicesDto[]>(this.apiUrl)
      .pipe(catchError(this.handleError));
  }

  getById(id: number): Observable<DeviceDto> {
    return this.http.get<DeviceDto>(`${this.apiUrl}/${id}`)
      .pipe(catchError(this.handleError));
  }

  create(device: CreateDeviceDto): Observable<BaseResponse<void>> {
    return this.http.post<BaseResponse<void>>(this.apiUrl, device)
      .pipe(
        catchError(this.handleError)
      );
  }



  update(id: number, device: UpdateDeviceDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, device)
      .pipe(catchError(this.handleError));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(catchError(this.handleError));
  }

  checkDuplicates(items: CheckDuplicateDto[]): Observable<BaseResponse<DuplicateCheckResponse>> {
    return this.http.post<BaseResponse<DuplicateCheckResponse>>(`${this.apiUrl}/check-duplicates`, items)
      .pipe(catchError(this.handleError));
  }


  uploadDevices(devices: DeviceUploadDto[]): Observable<BaseResponse<number>> {
    return this.http.post<BaseResponse<number>>(`${this.apiUrl}/upload-devices`, devices).pipe(
      catchError(this.handleError)
    );
  }

  restoreDevice(id: number): Observable<BaseResponse<number>> {
    return this.http.post<BaseResponse<number>>(`${this.apiUrl}/restore-device?id=${id}`, id).pipe(
      catchError(this.handleError)
    );
  }


  private handleError(error: HttpErrorResponse): Observable<never> {
    console.error('An error occurred:', error);
    let errorMessage = 'حدث خطأ غير متوقع';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `خطأ في العميل: ${error.error.message}`;
    } else {
      // Server-side error
      if (error.error?.message) {
        errorMessage = error.error.message;
      }
    }

    return throwError(() => new Error(errorMessage));
  }


}