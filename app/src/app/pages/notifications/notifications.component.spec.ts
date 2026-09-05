import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToastrService } from 'ngx-toastr';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { NotificationsComponent } from './notifications.component';
import { NotificationService } from '../../services/notification.service';

describe('NotificationsComponent', () => {
  let fixture: ComponentFixture<NotificationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotificationsComponent],
      providers: [
        provideRouter([]),
        { provide: ToastrService, useValue: jasmine.createSpyObj('ToastrService', ['success', 'error', 'warning', 'info']) },
        {
          provide: NotificationService,
          useValue: {
            getNotifications: () => of({ items: [], total: 0, page: 1, pageSize: 20, totalPages: 0 }),
            markAllRead: () => of(void 0),
            markRead: () => of(void 0),
            delete: () => of(void 0)
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationsComponent);
  });

  it('loads notification list', () => {
    fixture.detectChanges();
    expect(fixture.componentInstance.notifications).toEqual([]);
  });
});
