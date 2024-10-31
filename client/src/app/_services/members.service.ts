import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of } from 'rxjs/internal/observable/of';
import { tap } from 'rxjs';
import { Photo } from '../_models/photo';
import { UserParams } from '../_models/userParams';
import { User } from '../_models/user';
import { AccountService } from './account.service';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  private presence = inject(PresenceService);
  private accountService = inject(AccountService);
  baseUrl = environment.apiUrl;
  members = signal<Member[]>([]);
  memberCache = new Map();
  userParams: UserParams = new UserParams(this.accountService.currentUser()!);

  getMembers(){
    return this.http.get<Member[]>(this.baseUrl + 'users').subscribe({
      next: members => this.members.set(members)
    });
  }

  getUserParams() {
    return this.userParams;
  }

  getMember(username: string){
    const member = this.members().find(x => x.username == username);
    if(member !== undefined) return of(member);
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
