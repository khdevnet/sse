import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { WeatherForecast, WeatherForecastService } from '../weather-forecast.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent implements OnInit, OnDestroy {
  private sub = new Subscription();
  public forecasts: WeatherForecast[];

  constructor(private weatherForecastService: WeatherForecastService, private ref: ChangeDetectorRef) {
    ref.detach();
  }

  ngOnInit(): void {
    this.weatherForecastService.connect();
    this.sub.add(this.weatherForecastService.get().subscribe(result => {
      this.forecasts = result;
      console.log(result);
      this.ref.detectChanges();
    }, error => console.error(error)));
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
    this.weatherForecastService.stopExchangeUpdates();
  }
}