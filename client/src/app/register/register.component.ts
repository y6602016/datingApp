import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
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
  registerForm: FormGroup;
  maxDate: Date;

  constructor(private accountService: AccountService, private fb: FormBuilder) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }


  initializeForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      // this.matchValue('password'), password here correspond to below matchTo for controls[matchTo]
      confirmPassword: ['', [Validators.required, this.matchValue('password')]]
    })
    // when password is typed and changed, confirmPassword need to update and validate it again
    this.registerForm.controls.password.valueChanges.subscribe(() => {
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    })
  }

  // return a validation function
  matchValue(matchTo: string) : ValidatorFn {
    // formControl derive from AbstractControl
    return (control: AbstractControl) => {
      // if password match, then return null, 
      // otherwise attach a validator error called "isMatching" to the control, and it will fail the validation
      return control?.value === control?.parent?.controls[matchTo].value 
      ? null : {isMatching: true}
      // isMatching is used in template file's hasError('isMatching')
    }
  }

  register() {
    console.log(this.registerForm.value);
    // this.accountService.register(this.model).subscribe(response => {
    //   console.log(response);
    //   this.cancel();
    // }, error => {
    //   console.log(error);
    // })
  }

  // send value to parent component
  cancel() {
    this.cancelRegister.emit(false);
  }
}
