import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;

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
  }

  stopHubConnection() {
    this.hubConnection.stop().catch(error => console.log(error));
  }

}
