import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // <<<this is only for parent-to-child example>>>
  // use @Input to receive properties from parent component
  // @Input() usersFromHomeComponent: any;

  // use @Output to send properties from parent component
  @Output() cancelRegister = new EventEmitter();


  model: any = {};

  constructor(private accountService: AccountService) { }

  ngOnInit(): void {
  }

  register() {
    this.accountService.register(this.model).subscribe(response => {
      console.log(response);
      this.cancel();
    }, error => {
      console.log(error);
    })
  }

  // send value to parent component
  cancel() {
    this.cancelRegister.emit(false);
  }
}
