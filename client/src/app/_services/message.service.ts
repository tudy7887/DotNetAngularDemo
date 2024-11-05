import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { Message } from '../_models/message';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);

  getMesages(pageNumber:  number, pageSize: number, container: string){
    let params = setPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return this.http.get<Message[]>(`${this.baseUrl}messages`, {observe: 'response', params}).subscribe({
      next: response => setPaginatedResponse(response, this.paginatedResult)
    });
  }
}
