import { NgIf } from '@angular/common';
import { Component, input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';
import { BsDatepickerConfig, BsDatepickerModule } from 'ngx-bootstrap/datepicker';

@Component({
  selector: 'app-date-input',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, BsDatepickerModule],
  templateUrl: './date-input.component.html',
  styleUrl: './date-input.component.css'
})
export class DateInputComponent implements ControlValueAccessor {
  label = input<string>('');
  maxDate = input<Date>();
  bsConfig: Partial<BsDatepickerConfig>;

  constructor(@Self() public ngControl: NgControl) { 
    this.ngControl.valueAccessor = this;
    this.bsConfig = {
      containerClass: 'theme-angular-demo',
      dateInputFormat: 'DD MMMM YYYY'
    }
  }

  writeValue(obj: any): void {
  }

  registerOnChange(fn: any): void {
  }

  registerOnTouched(fn: any): void {
  }

  get control(): FormControl {
    return this.ngControl.control as FormControl;
  }
}
