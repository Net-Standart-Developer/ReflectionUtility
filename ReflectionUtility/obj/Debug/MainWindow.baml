   M S B A M L   `   `   `  ����L  HReflectionUtility, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:ReflectionUtilityReflectionUtility  5   	   "    ReflectionUtility.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:ReflectionUtility     5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)    � WindowStartupLocation$ CenterScreen=�5	   	    � WindowState$ 	Maximized=�6.   .��5
      �  � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $
��auto �6   �� 5      $
��1.5* �6   $	��450��6,   .+�5   
   � $��0��6    � RowDefinitions 5      �� 5      $
��auto �6    �� 5      $
��auto �6    �� 5      .+�5      �� $��1��6   .+�5      �� -   5        $
��Dlls��5      $��
10 10 10 0q�6'   6;   �� 5      -   5         ��Name$
 Type��5      $��
10 10 10 0q�6(    %� VirtualizationMode$ 	Recycling=�6[   5      �� 5      -   5         ��Name$
 list��5      $��2��6M   $��
10 10 10 0q�6Z   5      5      �� 5    
   $��1��6   $��Stretch=�6'   $��Center=�6C   $��1q�6`   
���   �6t   �� 5!   
   -   5        $
 text��5!       �� TextWrapping$
 Wrap=�6    $��2��64   	 �� 
IsReadOnly	 . $��
10 10 10 0q�6V   "
 �� VerticalScrollBarVisibility$

 Auto=�6j   5"      5#      