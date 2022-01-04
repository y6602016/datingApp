import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  // passing property down to child component(app-register) should be done here
  // we difine user here, and home template uses this property and pass it to children
  // <app-register [usersFromHomeComponent]="users"></app-register>
  users: any;

  registerMode = false;


  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.getUsers();
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  getUsers() {
    // http request return observable, and observable is lazy
    // we need to call subscribe to operate the data
    this.http.get('https://localhost:5001/api/users').subscribe(users => {
      this.users = users;
    }, error => {
      console.log(error);
    })
  }

}
