import { Component, Input, OnInit, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

// this component is our customized ngControl, we can reuse it as a component to replace many repearted 
// ngControl in the template file with reactiveForm. 
// in parent template file, we can use it as a ngControl like 
// <app-text-input [formControl]='registerForm.controls["username"]'[label]='"Username"'></app-text-input>
@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})
export class TextInputComponent implements ControlValueAccessor {
  @Input() label: string;
  @Input() type: 'text';

  // inject control into the constroctor of the ControlValueAccessor componenet
  constructor(@Self() public ngControl: NgControl) {
    this.ngControl.valueAccessor = this;
  }

  writeValue(obj: any): void {

  }
  registerOnChange(fn: any): void {

  }
  registerOnTouched(fn: any): void {

  }
}
