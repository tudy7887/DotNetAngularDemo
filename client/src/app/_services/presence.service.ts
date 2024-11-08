import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../environments/environment';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/user';
import { take } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  private router = inject(Router);
  private toastr = inject(ToastrService);
  private hubConnection?: HubConnection
  hubsUrl = environment.hubsUrl;
  onlineUsers = signal<string[]>([])

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubsUrl + 'presence', {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build()
    this.hubConnection.start()
    .catch(error => console.log(error));

    this.hubConnection.on('UserIsOnline', username => {
      this.toastr.info(username + ' has connected');
    })

    this.hubConnection.on('UserIsOffline', username => {
      this.toastr.warning(username + ' has disconected');
    })

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsers.set(usernames);
    })

    // this.hubConnection.on('NewMessageReceived', ({username, knownAs}) => {
    //   this.toastr.info(knownAs + ' has sent you a new message!')
    //     .onTap
    //     .pipe(take(1))
    //     .subscribe(() => this.router.navigateByUrl('/members/' + username + '?tab=3'));
    // })
  }

  stopHubConnection() {
    if(this.hubConnection?.state === HubConnectionState.Connected){
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }
}
