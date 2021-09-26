import { Inject, Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";

@Injectable({
  providedIn: "root"
})
export class WeatherForecastService {
  private evs: EventSource;
  private subj = new BehaviorSubject<WeatherForecast[]>([]);

  constructor(@Inject('BASE_URL') private baseUrl: string) {
  }

  get() {
    return this.subj.asObservable();
  }

  connect() {
    let subject = this.subj;
    if (typeof (EventSource) !== 'undefined') {
      this.evs = new EventSource(this.baseUrl + 'weatherforecast');
      this.evs.onopen = function (e) {
        console.log('Opening connection.Ready State is ' + this.readyState);
      }
      this.evs.onmessage = function (e) {
        console.log('Message Received.Ready State is ' + this.readyState);
        subject.next(JSON.parse(e.data));
      }
      this.evs.addEventListener("timestamp", function (e) {
        console.log("Timestamp event Received.Ready State is " + this.readyState);
        //subject.next(e["data"]);
      })
      this.evs.onerror = function (e) {
        console.log(e);
        if (this.readyState == 0) {
          console.log('Reconnecting…');
        }
      }
    }
  }

  stopExchangeUpdates() {
    this.evs.close();
  }
}

export interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}
