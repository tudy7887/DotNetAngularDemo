import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, model, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of } from 'rxjs/internal/observable/of';
import { Photo } from '../_models/photo';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { PaginatedResult } from '../_models/pagination';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  //private presence = inject(PresenceService);
  members = signal<Member[]>([]);
  private accountService = inject(AccountService);
  user = this.accountService.currentUser();
  memberCache = new Map();
  userParams = signal<UserParams>(new UserParams(this.user));
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);

  resetUserParams() {
    this.userParams.set(new UserParams(this.user));
  }

  getUserParams() {
    //return this.userParams;
  }

  setUserParams(params: UserParams) {
    //this.userParams = params;
  }

  getMembers() {
    var response = this.memberCache.get(Object.values(this.userParams()).join('-'));
    if (response) {
      return setPaginatedResponse(response, this.paginatedResult);
    }

    let params = setPaginationHeaders(this.userParams().pageNumber, this.userParams().pageSize);
    params = params.append('minAge', this.userParams().minAge);
    params = params.append('maxAge', this.userParams().maxAge);
    params = params.append('gender', this.userParams().gender);
    params = params.append('orderBy', this.userParams().orderBy);

    return this.http.get<Member[]>(this.baseUrl + 'users', { observe: 'response', params }).subscribe({
      next: response => {
        setPaginatedResponse(response, this.paginatedResult);
        this.memberCache.set(Object.values(this.userParams()).join('-'), response)
      }
    });
  }

  getMember(username: string) {
    const member: Member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((member: Member) => member.username === username);

    if (member) {
      return of(member);
    }
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member){
    return this.http.put(this.baseUrl + 'users', member).pipe(tap(() => {
      this.members.update(members => 
                          members.map(m => m.username === member.username ? member : m))
    }));
  }

  setMainPhoto(photo: Photo){
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {}).pipe(tap(() => {
      this.members.update(members => members.map(m => {
        if(m.photos.includes(photo)){
          m.photoUrl = photo.url;
        }
        return m;
      }))
    }));
  }

  deletePhoto(photo: Photo){
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id, {}).pipe(tap(() => {
      this.members.update(members => members.map(m => {
        if(m.photos.includes(photo)){
          m.photos = m.photos.filter(p => p.id != photo.id);
        }
        return m;
      }))
    }));;
  }
}