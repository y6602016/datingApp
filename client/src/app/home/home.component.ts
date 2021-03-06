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

  // <app-register [usersFromHomeComponent]="users" (cancelRegister)="cancelRegisterMode($event)">

  // [usersFromHomeComponent]="users" in this child component means child receives 
  // value of usersFromHomeComponent from parent's "users" variable

  // (cancelRegister)="cancelRegisterMode($event)"" means child sends event as parameter 
  // to parent's cancelRegisterMode method

  // <<<this is only for parent-to-child example>>>
  // users: any;

  registerMode = false;


  constructor() { }

  ngOnInit(): void {
    // this.getUsers();
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  // <<<this is only for parent-to-child example>>
  // getUsers() {
  //   // http request return observable, and observable is lazy
  //   // we need to call subscribe to operate the data
  //   this.http.get('https://localhost:5001/api/users').subscribe(users => {
  //     this.users = users;
  //   }, error => {
  //     console.log(error);
  //   })
  // }

  // receive event from child component
  cancelRegisterMode(event: boolean) {
    this.registerMode = event;
  }
}
