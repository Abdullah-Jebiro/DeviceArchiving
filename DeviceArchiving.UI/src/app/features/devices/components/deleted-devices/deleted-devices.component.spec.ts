import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeletedDevicesComponent } from './deleted-devices.component';

describe('DeletedDevicesComponent', () => {
  let component: DeletedDevicesComponent;
  let fixture: ComponentFixture<DeletedDevicesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DeletedDevicesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeletedDevicesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
