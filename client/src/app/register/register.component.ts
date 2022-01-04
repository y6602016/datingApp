import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // use @Input to receive properties from parent component
  @Input() usersFromHomeComponent: any;

  // use @Output to send properties from parent component
  @Output() cancelRegister = new EventEmitter();


  model: any = {};

  constructor() { }

  ngOnInit(): void {
  }

  register() {
    console.log(this.model);
  }

  // send value to parent component
  cancel() {
    this.cancelRegister.emit(false);
  }
}
