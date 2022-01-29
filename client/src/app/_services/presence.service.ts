import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;

  // store username array and initialize with an empty array
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  // use above to create an observable
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService) { }

  // create the hub connection such that when a user connect to the application and pass the auth,
  // automatically create a hub connection to connect them and the presence hub
  createHubConnection(user: User) {
    // build the connection
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', { //the endpoint
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build()

    // start the connection
    this.hubConnection
      .start()
      .catch(error => console.log(error));

    // check connection status then use toastr, the status need to match the variable we dedine in
    // PresenceHub.cs file
    this.hubConnection.on('UserIsOnline', username => {
      this.toastr.info(username + ' has connected');
    })

    this.hubConnection.on('UserIsOffline', username => {
      this.toastr.warning(username + ' has disconnected');
    })

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsersSource.next(usernames);
    })
  }

  stopHubConnection() {
    this.hubConnection.stop().catch(error => console.log(error));
  }

}
